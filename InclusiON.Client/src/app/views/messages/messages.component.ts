import { Component, inject, OnInit, signal, computed, ViewChild, ElementRef, effect } from '@angular/core';
import { UserRoles } from '@shared/constants/roles';
import { DatePipe } from '@angular/common';
import { ActorAvatarComponent } from '@shared/components/actor-avatar/actor-avatar.component';
import { FormsModule } from '@angular/forms';
import {
  ColComponent, RowComponent, SpinnerComponent,
  ButtonDirective, FormControlDirective,
} from '@coreui/angular';
import { IconDirective } from '@coreui/icons-angular';
import {
  MessagesService,
  MessageListItemResponse,
  MessageDetailResponse,
  MessageContactResponse,
} from '@services/messages.service';
import { ToastService, AuthService } from '@services';

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
  ],
  templateUrl: './messages.component.html',
  styleUrl: './messages.component.scss',
})
export class MessagesComponent implements OnInit {
  private readonly messagesService = inject(MessagesService);
  private readonly toastService    = inject(ToastService);
  private readonly authService     = inject(AuthService);

  @ViewChild('chatHistory') chatHistory!: ElementRef<HTMLDivElement>;

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
  selectedContact = signal<MessageContactResponse | null>(null);
  selectedDetail = signal<MessageDetailResponse | null>(null);
  chatMessages = signal<any[]>([]);

  loadingContacts = signal(true);
  loadingChat = signal(false);
  sendingMessage = signal(false);

  newMessageText = '';

  // Get current user ID to identify our sent messages
  currentUserId = '';

  // Computed signal to filter contacts by search query
  filteredContacts = computed(() => {
    const query = this.searchQuery().toLowerCase().trim();
    const allContacts = this.contacts();
    if (!query) return allContacts;
    return allContacts.filter(c => c.fullName.toLowerCase().includes(query));
  });

  // ── Init ───────────────────────────────────────────────────────────────
  ngOnInit(): void {
    this.currentUserId = this.authService.getCurrentUser()?.id || '';
    this.loadContacts();
  }

  // ── Loaders ────────────────────────────────────────────────────────────
  loadContacts(): void {
    this.loadingContacts.set(true);
    this.messagesService.getContacts(1, 100).subscribe({
      next: (list) => {
        this.contacts.set(list);
        this.loadingContacts.set(false);

        // Pre-select first contact if available
        if (list.length > 0) {
          this.loadChatForContact(list[0]);
        }
      },
      error: () => {
        this.toastService.error('Error al cargar contactos');
        this.loadingContacts.set(false);
      },
    });
  }

  loadChatForContact(contact: MessageContactResponse): void {
    this.selectedContact.set(contact);
    this.loadingChat.set(true);
    this.chatMessages.set([]);
    this.selectedDetail.set(null);
    this.newMessageText = '';

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
          this.loadChatForContact(contact);
        },
        error: () => {
          this.toastService.error('Error al enviar mensaje');
          this.sendingMessage.set(false);
        }
      });
    }
  }

  // Helper labels
  contactLabel(c: MessageContactResponse): string {
    return c.userType === UserRoles.Professional ? 'Profesional' : 'Familiar/Tutor';
  }
}
