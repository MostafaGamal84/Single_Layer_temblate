import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class GameSessionService {
  private base = `${environment.apiBaseUrl}/game-sessions`;
  constructor(private http: HttpClient) {}

  create(payload: { quizId: number; questionFlowMode: number }) { return this.http.post<any>(this.base, payload); }
  getAll() { return this.http.get<any[]>(this.base); }
  getById(id: number) { return this.http.get<any>(`${this.base}/${id}`); }
  getByCode(code: string) { return this.http.get<any>(`${this.base}/by-code/${code}`); }
  start(id: number) { return this.http.post(`${this.base}/${id}/start`, {}); }
  pause(id: number) { return this.http.post(`${this.base}/${id}/pause`, {}); }
  resume(id: number) { return this.http.post(`${this.base}/${id}/resume`, {}); }
  end(id: number) { return this.http.post(`${this.base}/${id}/end`, {}); }
  delete(id: number) { return this.http.delete(`${this.base}/${id}`); }
  state(id: number) { return this.http.get<any>(`${this.base}/${id}/state`); }
  nextQuestion(id: number) { return this.http.post(`${this.base}/${id}/next-question`, {}); }
  leaderboard(id: number) { return this.http.get<any[]>(`${this.base}/${id}/leaderboard`); }
  joinRequests(id: number) { return this.http.get<any[]>(`${this.base}/${id}/join-requests`); }
  approveJoinRequest(id: number, participantId: number) { return this.http.post<any>(`${this.base}/${id}/join-requests/${participantId}/approve`, {}); }
  rejectJoinRequest(id: number, participantId: number, decisionNote?: string) {
    return this.http.post<any>(`${this.base}/${id}/join-requests/${participantId}/reject`, { decisionNote: decisionNote || null });
  }
}
