import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiResponse, PagedResponse } from '@models';
import { environment } from '@env';
import { unwrapResponse } from '@shared/utils';

// ── List item (inbox / sent / replies list) ──────────────────────────────────
export interface MessageListItemResponse {
  id: number;
  subject?: string;
  contentPreview: string;
  sentAt: string;
  readAt?: string;
  isRead: boolean;
  senderId: string;
  senderFullName: string;
  receiverId: string;
  receiverFullName: string;
  relatedPersonId?: string;
  parentMessageId?: number;
  replyCount: number;
}

// ── Full detail (GET /messages/:id) ─────────────────────────────────────────
export interface MessageDetailResponse {
  id: number;
  subject?: string;
  content: string;
  sentAt: string;
  readAt?: string;
  isRead: boolean;
  senderId: string;
  senderFullName: string;
  receiverId: string;
  receiverFullName: string;
  relatedPersonId?: string;
  parentMessageId?: number;
  replies: MessageListItemResponse[];
}

// ── Contact ──────────────────────────────────────────────────────────────────
export interface MessageContactResponse {
  userId: string;
  fullName: string;
  email: string;
  userType: string;  // "Professional" | "FamilyRepresentative"
}

// ── Requests ─────────────────────────────────────────────────────────────────
export interface SendMessageRequest {
  receiverId: string;
  subject: string;
  content: string;
  relatedPersonId?: string;
}

@Injectable({ providedIn: 'root' })
export class MessagesService {
  private readonly http = inject(HttpClient);

  private get baseUrl(): string {
    return `${environment.apiUrl}/Messages`;
  }

  getInbox(params?: {
    page?: number; pageSize?: number;
    isRead?: boolean; relatedPersonId?: string; senderId?: string;
  }): Observable<PagedResponse<MessageListItemResponse>> {
    let p = new HttpParams()
      .set('page',     params?.page?.toString()     ?? '1')
      .set('pageSize', params?.pageSize?.toString() ?? '20');
    if (params?.isRead !== undefined)     p = p.set('isRead',          params.isRead.toString());
    if (params?.relatedPersonId)          p = p.set('relatedPersonId', params.relatedPersonId);
    if (params?.senderId)                 p = p.set('senderId',        params.senderId);

    return this.http
      .get<ApiResponse<PagedResponse<MessageListItemResponse>>>(`${this.baseUrl}/inbox`, { params: p })
      .pipe(unwrapResponse());
  }

  getSent(params?: {
    page?: number; pageSize?: number;
    isRead?: boolean; relatedPersonId?: string; receiverId?: string;
  }): Observable<PagedResponse<MessageListItemResponse>> {
    let p = new HttpParams()
      .set('page',     params?.page?.toString()     ?? '1')
      .set('pageSize', params?.pageSize?.toString() ?? '20');
    if (params?.isRead !== undefined) p = p.set('isRead',          params.isRead.toString());
    if (params?.relatedPersonId)      p = p.set('relatedPersonId', params.relatedPersonId);
    if (params?.receiverId)           p = p.set('receiverId',      params.receiverId);

    return this.http
      .get<ApiResponse<PagedResponse<MessageListItemResponse>>>(`${this.baseUrl}/sent`, { params: p })
      .pipe(unwrapResponse());
  }

  // Auto-marks as read on backend when recipient opens the message
  getById(id: number): Observable<MessageDetailResponse> {
    return this.http
      .get<ApiResponse<MessageDetailResponse>>(`${this.baseUrl}/${id}`)
      .pipe(unwrapResponse());
  }

  getContacts(): Observable<MessageContactResponse[]> {
    return this.http
      .get<ApiResponse<MessageContactResponse[]>>(`${this.baseUrl}/contacts`)
      .pipe(unwrapResponse());
  }

  getUnreadCount(): Observable<number> {
    return this.http
      .get<ApiResponse<{ count: number }>>(`${this.baseUrl}/unread-count`)
      .pipe(map(res => res.data?.count ?? 0));
  }

  send(request: SendMessageRequest): Observable<MessageDetailResponse> {
    return this.http
      .post<ApiResponse<MessageDetailResponse>>(this.baseUrl, request)
      .pipe(unwrapResponse());
  }

  reply(id: number, body: string): Observable<MessageDetailResponse> {
    return this.http
      .post<ApiResponse<MessageDetailResponse>>(`${this.baseUrl}/${id}/reply`, { content: body })
      .pipe(unwrapResponse());
  }

  // Manual mark-as-read (backend also does it automatically on getById for recipients)
  markAsRead(id: number): Observable<MessageDetailResponse> {
    return this.http
      .put<ApiResponse<MessageDetailResponse>>(`${this.baseUrl}/${id}/read`, {})
      .pipe(unwrapResponse());
  }
}
