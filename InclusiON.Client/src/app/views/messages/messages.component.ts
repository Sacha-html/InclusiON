import { Component, inject, OnInit, OnDestroy, signal, computed, ViewChild, ElementRef, effect } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { UserRoles } from '@shared/constants/roles';
import { DatePipe } from '@angular/common';
import { ActorAvatarComponent } from '@shared/components/actor-avatar/actor-avatar.component';
import { FormsModule } from '@angular/forms';
import { Subscription } from 'rxjs';
import {
  ColComponent, RowComponent, SpinnerComponent,
  ButtonDirective, FormControlDirective, BadgeComponent
} from '@coreui/angular';
import { IconDirective } from '@coreui/icons-angular';
import {
  MessagesService,
  MessageListItemResponse,
  MessageDetailResponse,
  MessageContactResponse,
} from '@services/messages.service';
import { ToastService, AuthService, SignalrService } from '@services';

@Component({
  selector: 'app-messages',
  standalone: true,
  imports: [
    FormsModule,
    ActorAvatarComponent,
    DatePipe,
    RowComponent, ColComponent,
    SpinnerComponent,
    ButtonDirective,
    FormControlDirective,
    IconDirective,
    BadgeComponent,
  ],
  templateUrl: './messages.component.html',
  styleUrl: './messages.component.scss',
})
export class MessagesComponent implements OnInit, OnDestroy {
  private readonly messagesService = inject(MessagesService);
  private readonly toastService    = inject(ToastService);
  private readonly authService     = inject(AuthService);
  private readonly signalrService   = inject(SignalrService);
  private readonly route            = inject(ActivatedRoute);

  @ViewChild('chatHistory') chatHistory!: ElementRef<HTMLDivElement>;

  private notificationSub?: Subscription;
  private routeQuerySub?: Subscription;

  constructor() {
    effect(() => {
      const msgs = this.chatMessages();
      if (msgs.length > 0) {
        this.scrollToBottom();
      }
    });
  }

  scrollToBottom(): void {
    setTimeout(() => {
      if (this.chatHistory) {
        const element = this.chatHistory.nativeElement;
        element.scrollTop = element.scrollHeight;
      }
    }, 100);
  }

  // ── Signals ────────────────────────────────────────────────────────────
  contacts = signal<MessageContactResponse[]>([]);
  searchQuery = signal<string>('');
  contactTypeFilter = signal<'ALL' | 'PROFESSIONAL' | 'FAMILY'>('ALL');
  selectedContact = signal<MessageContactResponse | null>(null);
  selectedDetail = signal<MessageDetailResponse | null>(null);
  chatMessages = signal<any[]>([]);

  loadingContacts = signal(true);
  loadingChat = signal(false);
  sendingMessage = signal(false);

  newMessageText = '';

  // Get current user ID to identify our sent messages
  currentUserId = '';

  isAdmin = computed(() => {
    const user = this.authService.getCurrentUser();
    return user?.role === UserRoles.Admin || this.authService.isGlobalAdmin();
  });

  // Computed signal to filter contacts by search query and type filter
  filteredContacts = computed(() => {
    const query = this.searchQuery().toLowerCase().trim();
    const typeFilter = this.contactTypeFilter();
    let list = this.contacts();

    if (typeFilter === 'PROFESSIONAL') {
      list = list.filter(c => c.userType === UserRoles.Professional || c.userType === 'Professional');
    } else if (typeFilter === 'FAMILY') {
      list = list.filter(c => c.userType === UserRoles.FamilyRepresentative || c.userType === 'FamilyRepresentative' || c.userType === 'Family');
    }

    if (!query) return list;
    return list.filter(c => c.fullName.toLowerCase().includes(query) || (c.email && c.email.toLowerCase().includes(query)));
  });

  setContactTypeFilter(filter: 'ALL' | 'PROFESSIONAL' | 'FAMILY'): void {
    this.contactTypeFilter.set(filter);
  }

  // ── Init ───────────────────────────────────────────────────────────────
  ngOnInit(): void {
    this.currentUserId = this.authService.getCurrentUser()?.id || '';
    this.loadContacts();
    this.listenToIncomingMessages();
    this.listenToRouteQueryParams();
  }

  ngOnDestroy(): void {
    this.notificationSub?.unsubscribe();
    this.routeQuerySub?.unsubscribe();
  }

  private listenToRouteQueryParams(): void {
    this.routeQuerySub = this.route.queryParams.subscribe(params => {
      const targetId = params['contactId'] || params['userId'];
      if (targetId) {
        const contactList = this.contacts();
        if (contactList.length > 0) {
          const found = contactList.find(c => c.userId.toLowerCase() === targetId.toLowerCase());
          if (found && this.selectedContact()?.userId !== found.userId) {
            this.loadChatForContact(found);
          }
        }
      }
    });
  }

  private listenToIncomingMessages(): void {
    this.notificationSub = this.signalrService.notification$.subscribe((notif) => {
      // Si la notificación es de mensajería
      const selected = this.selectedContact();
      this.loadContactsSilent();

      if (selected) {
        // Refrescar el chat activo si hay interacción
        this.loadChatForContact(selected, false);
      }
    });
  }

  // ── Loaders ────────────────────────────────────────────────────────────
  loadContacts(autoSelectFirst: boolean = true): void {
    this.loadingContacts.set(true);
    const targetContactId = this.route.snapshot.queryParamMap.get('contactId') || this.route.snapshot.queryParamMap.get('userId');

    this.messagesService.getContacts(1, 100).subscribe({
      next: (list) => {
        this.contacts.set(list);
        this.loadingContacts.set(false);

        // Si viene un contactId por URL, seleccionarlo con prioridad
        if (targetContactId) {
          const matched = list.find(c => c.userId.toLowerCase() === targetContactId.toLowerCase());
          if (matched) {
            this.loadChatForContact(matched);
            return;
          }
        }

        // Pre-select first contact if available
        if (autoSelectFirst && list.length > 0 && !this.selectedContact()) {
          this.loadChatForContact(list[0]);
        }
      },
      error: () => {
        this.toastService.error('Error al cargar contactos');
        this.loadingContacts.set(false);
      },
    });
  }

  private loadContactsSilent(): void {
    this.messagesService.getContacts(1, 100).subscribe({
      next: (list) => {
        this.contacts.set(list);
      },
    });
  }

  loadChatForContact(contact: MessageContactResponse, resetUnread: boolean = true): void {
    this.selectedContact.set(contact);
    this.loadingChat.set(true);
    this.chatMessages.set([]);
    this.selectedDetail.set(null);
    this.newMessageText = '';

    // Marcar como leído en UI y backend si tiene mensajes pendientes
    if (resetUnread && ((contact.mensajesNoLeidos || 0) > 0 || (contact.unreadCount || 0) > 0)) {
      this.contacts.update(currentList =>
        currentList.map(c => c.userId === contact.userId
          ? { ...c, mensajesNoLeidos: 0, unreadCount: 0 }
          : c
        )
      );

      this.messagesService.markConversationAsRead(contact.userId).subscribe({
        error: (err) => console.error('Error al marcar conversación como leída:', err)
      });
    }

    // Fetch inbox messages to check for an existing thread
    this.messagesService.getInbox({ page: 1, pageSize: 50, senderId: contact.userId }).subscribe({
      next: (inboxRes) => {
        const inboxParent = inboxRes.data.find(m => !m.parentMessageId);
        if (inboxParent) {
          this.loadFullThread(inboxParent.encryptedId);
          return;
        }

        // Check sent messages if no inbox parent exists
        this.messagesService.getSent({ page: 1, pageSize: 50, receiverId: contact.userId }).subscribe({
          next: (sentRes) => {
            const sentParent = sentRes.data.find(m => !m.parentMessageId);
            if (sentParent) {
              this.loadFullThread(sentParent.encryptedId);
            } else {
              this.loadingChat.set(false);
            }
          },
          error: () => {
            this.loadingChat.set(false);
          }
        });
      },
      error: () => {
        this.loadingChat.set(false);
      }
    });
  }

  private loadFullThread(encryptedId: string): void {
    this.messagesService.getById(encryptedId).subscribe({
      next: (detail) => {
        this.selectedDetail.set(detail);
        const messages: any[] = [{
          id: detail.id,
          encryptedId: detail.encryptedId,
          content: detail.content,
          sentAt: detail.sentAt,
          senderId: detail.senderId,
          senderFullName: detail.senderFullName,
          receiverId: detail.receiverId,
          receiverFullName: detail.receiverFullName
        }];

        if (detail.replies) {
          detail.replies.forEach(r => {
            messages.push({
              id: r.id,
              encryptedId: r.encryptedId,
              content: r.content,
              sentAt: r.sentAt,
              senderId: r.senderId,
              senderFullName: r.senderFullName,
              receiverId: r.receiverId,
              receiverFullName: r.receiverFullName
            });
          });
        }

        messages.sort((a, b) => new Date(a.sentAt).getTime() - new Date(b.sentAt).getTime());
        this.chatMessages.set(messages);
        this.loadingChat.set(false);
      },
      error: () => {
        this.toastService.error('Error al cargar conversación');
        this.loadingChat.set(false);
      }
    });
  }

  // ── Send Message ───────────────────────────────────────────────────────
  sendChatMessage(): void {
    const contact = this.selectedContact();
    if (!contact || !this.newMessageText.trim()) return;

    const text = this.newMessageText.trim();
    this.newMessageText = '';
    this.sendingMessage.set(true);

    // Mover contacto a la posición [0] inmediatamente en el frontend
    this.moveContactToTop(contact.userId);

    const detail = this.selectedDetail();
    if (detail) {
      this.messagesService.reply(detail.encryptedId, text).subscribe({
        next: () => {
          this.sendingMessage.set(false);
          this.loadFullThread(detail.encryptedId);
        },
        error: () => {
          this.toastService.error('Error al enviar respuesta');
          this.sendingMessage.set(false);
        }
      });
    } else {
      this.messagesService.send({
        receiverId: contact.userId,
        subject: 'Chat de InclusiON',
        content: text
      }).subscribe({
        next: () => {
          this.sendingMessage.set(false);
          this.loadChatForContact(contact, false);
        },
        error: () => {
          this.toastService.error('Error al enviar mensaje');
          this.sendingMessage.set(false);
        }
      });
    }
  }

  private moveContactToTop(contactUserId: string): void {
    const nowIso = new Date().toISOString();
    this.contacts.update(list => {
      const idx = list.findIndex(c => c.userId === contactUserId);
      if (idx === -1) return list;
      const contact = { ...list[idx], ultimoMensajeFecha: nowIso, lastMessageDate: nowIso };
      const rest = list.filter((_, i) => i !== idx);
      return [contact, ...rest];
    });
  }

  // Helper labels
  contactLabel(c: MessageContactResponse): string {
    if (c.userType === UserRoles.Admin || c.userType === 'Admin') return 'Administrador';
    if (c.userType === UserRoles.Professional || c.userType === 'Professional') return 'Profesional';
    return 'Familiar / Tutor';
  }
}
