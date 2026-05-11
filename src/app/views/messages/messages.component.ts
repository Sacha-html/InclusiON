import { Component, inject, OnInit, signal } from '@angular/core';
import { UserRoles } from '@shared/constants/roles';
import { DatePipe } from '@angular/common';
import { ActorAvatarComponent } from '@shared/components/actor-avatar/actor-avatar.component';
import { FormsModule } from '@angular/forms';
import {
  CardBodyComponent, CardComponent, CardHeaderComponent,
  ColComponent, RowComponent, BadgeComponent, SpinnerComponent,
  ButtonDirective, FormLabelDirective, FormControlDirective,
  FormSelectDirective, ModalComponent, ModalHeaderComponent,
  ModalBodyComponent, ModalFooterComponent, ModalTitleDirective,
  AlertComponent,
} from '@coreui/angular';
import { IconDirective } from '@coreui/icons-angular';
import {
  MessagesService,
  MessageListItemResponse,
  MessageDetailResponse,
  MessageContactResponse,
} from '../../services/messages.service';
import { ToastService } from '@services';

type ActiveTab = 'inbox' | 'sent';

@Component({
  selector: 'app-messages',
  standalone: true,
  imports: [
    FormsModule,
    ActorAvatarComponent,
    DatePipe,
    CardComponent, CardBodyComponent, CardHeaderComponent,
    RowComponent, ColComponent,
    BadgeComponent, SpinnerComponent,
    ButtonDirective,
    FormLabelDirective, FormControlDirective, FormSelectDirective,
    ModalComponent, ModalHeaderComponent, ModalBodyComponent,
    ModalFooterComponent, ModalTitleDirective,
    AlertComponent,
    IconDirective,
  ],
  templateUrl: './messages.component.html',
  styleUrl: './messages.component.scss',
})
export class MessagesComponent implements OnInit {
  private readonly messagesService = inject(MessagesService);
  private readonly toastService    = inject(ToastService);

  // ── State ──────────────────────────────────────────────────────────────
  activeTab: ActiveTab = 'inbox';

  inboxMessages  = signal<MessageListItemResponse[]>([]);
  sentMessages   = signal<MessageListItemResponse[]>([]);
  selectedDetail = signal<MessageDetailResponse | null>(null);

  loadingList   = signal(true);
  loadingDetail = signal(false);
  sendingReply  = signal(false);
  sendingNew    = signal(false);

  inboxTotal  = signal(0);
  sentTotal   = signal(0);
  currentPage = signal(1);
  readonly pageSize = 20;

  // Compose modal
  showCompose     = false;
  contacts        = signal<MessageContactResponse[]>([]);
  loadingContacts = false;
  compose         = { receiverId: '', subject: '', content: '' };
  composeError    = '';

  // Reply
  replyBody = '';

  // ── Init ───────────────────────────────────────────────────────────────
  ngOnInit(): void {
    this.loadInbox();
    this.loadContacts();
  }

  // ── Tab switch ─────────────────────────────────────────────────────────
  switchTab(tab: ActiveTab): void {
    if (this.activeTab === tab) return;
    this.activeTab = tab;
    this.selectedDetail.set(null);
    this.currentPage.set(1);
    tab === 'inbox' ? this.loadInbox() : this.loadSent();
  }

  // ── Loaders ────────────────────────────────────────────────────────────
  loadInbox(): void {
    this.loadingList.set(true);
    this.messagesService.getInbox({ page: this.currentPage(), pageSize: this.pageSize })
      .subscribe({
        next: (res) => {
          this.inboxMessages.set(res.data);
          this.inboxTotal.set(res.totalRecords);
          this.loadingList.set(false);
        },
        error: () => {
          this.toastService.error('Error al cargar bandeja de entrada');
          this.loadingList.set(false);
        },
      });
  }

  loadSent(): void {
    this.loadingList.set(true);
    this.messagesService.getSent({ page: this.currentPage(), pageSize: this.pageSize })
      .subscribe({
        next: (res) => {
          this.sentMessages.set(res.data);
          this.sentTotal.set(res.totalRecords);
          this.loadingList.set(false);
        },
        error: () => {
          this.toastService.error('Error al cargar mensajes enviados');
          this.loadingList.set(false);
        },
      });
  }

  // ── Message detail ─────────────────────────────────────────────────────
  openMessage(msg: MessageListItemResponse): void {
    this.loadingDetail.set(true);
    this.replyBody = '';
    this.messagesService.getById(msg.encryptedId).subscribe({
      next: (detail) => {
        this.selectedDetail.set(detail);
        this.loadingDetail.set(false);
        // Backend auto-marks as read on getById; update local list to reflect
        if (!msg.isRead && this.activeTab === 'inbox') {
          this.inboxMessages.update(list =>
            list.map(m => m.encryptedId === msg.encryptedId ? { ...m, isRead: true } : m)
          );
        }
      },
      error: () => {
        this.toastService.error('Error al cargar el mensaje');
        this.loadingDetail.set(false);
      },
    });
  }

  closeDetail(): void {
    this.selectedDetail.set(null);
    this.replyBody = '';
  }

  // ── Reply ──────────────────────────────────────────────────────────────
  sendReply(): void {
    const detail = this.selectedDetail();
    if (!detail || !this.replyBody.trim()) return;

    this.sendingReply.set(true);
    this.messagesService.reply(detail.encryptedId, this.replyBody.trim()).subscribe({
      next: (updated) => {
        this.selectedDetail.set(updated);
        this.replyBody = '';
        this.sendingReply.set(false);
        this.toastService.success('Respuesta enviada');
      },
      error: () => {
        this.toastService.error('Error al enviar respuesta');
        this.sendingReply.set(false);
      },
    });
  }

  // ── Compose ────────────────────────────────────────────────────────────
  openCompose(): void {
    this.compose     = { receiverId: '', subject: '', content: '' };
    this.composeError = '';
    this.showCompose  = true;
  }

  closeCompose(): void {
    this.showCompose = false;
  }

  sendNew(): void {
    if (!this.compose.receiverId || !this.compose.subject.trim() || !this.compose.content.trim()) {
      this.composeError = 'Todos los campos son obligatorios.';
      return;
    }
    this.composeError = '';
    this.sendingNew.set(true);
    this.messagesService.send({
      receiverId: this.compose.receiverId,
      subject:    this.compose.subject.trim(),
      content:    this.compose.content.trim(),
    }).subscribe({
      next: () => {
        this.showCompose = false;
        this.sendingNew.set(false);
        this.toastService.success('Mensaje enviado');
        if (this.activeTab === 'sent') this.loadSent();
      },
      error: () => {
        this.toastService.error('Error al enviar mensaje');
        this.sendingNew.set(false);
      },
    });
  }

  // ── Contacts ───────────────────────────────────────────────────────────
  private loadContacts(): void {
    this.loadingContacts = true;
    this.messagesService.getContacts().subscribe({
      next: (list) => {
        this.contacts.set(list);
        this.loadingContacts = false;
      },
      error: () => { this.loadingContacts = false; },
    });
  }

  // ── Pagination ─────────────────────────────────────────────────────────
  get totalPages(): number {
    const total = this.activeTab === 'inbox' ? this.inboxTotal() : this.sentTotal();
    return Math.ceil(total / this.pageSize);
  }

  prevPage(): void {
    if (this.currentPage() <= 1) return;
    this.currentPage.update(p => p - 1);
    this.activeTab === 'inbox' ? this.loadInbox() : this.loadSent();
  }

  nextPage(): void {
    if (this.currentPage() >= this.totalPages) return;
    this.currentPage.update(p => p + 1);
    this.activeTab === 'inbox' ? this.loadInbox() : this.loadSent();
  }

  // ── Helpers ────────────────────────────────────────────────────────────
  get currentList(): MessageListItemResponse[] {
    return this.activeTab === 'inbox' ? this.inboxMessages() : this.sentMessages();
  }

  unreadCount(): number {
    return this.inboxMessages().filter(m => !m.isRead).length;
  }

  contactLabel(c: MessageContactResponse): string {
    const type = c.userType === UserRoles.Professional ? 'Profesional' : 'Familiar';
    return `${c.fullName} (${type})`;
  }

}
