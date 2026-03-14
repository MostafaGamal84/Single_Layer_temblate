import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { QuizService } from '../../core/services/quiz.service';
import { GameSessionService } from '../../core/services/game-session.service';
import { SignalrService } from '../../core/services/signalr.service';

@Component({
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  template: `
    <div class="card list-shell">
      <h2>Live Game Sessions</h2>

      <div class="create-bar">
        <select [(ngModel)]="quizId">
          <option [ngValue]="0">Select game quiz</option>
          @for (q of quizzes; track q.id) {
            <option [ngValue]="q.id">{{ q.title }}</option>
          }
        </select>

        <select [(ngModel)]="questionFlowMode">
          <option [ngValue]="1">Host controls next question</option>
          <option [ngValue]="2">Timed by question duration</option>
        </select>

        <button type="button" (click)="create()">Create Session</button>
      </div>

      @if (error) { <div class="alert">{{ error }}</div> }

      <div class="table-wrap desktop-table">
        <table>
          <thead>
            <tr><th>Quiz</th><th>Code</th><th>Flow</th><th>Status</th><th>Participants</th><th>Actions</th></tr>
          </thead>
          <tbody>
            @for (s of sessions; track s.id) {
              <tr>
                <td>{{ s.quizTitle }}</td>
                <td>{{ s.joinCode }}</td>
                <td>{{ flowModeLabel(s.questionFlowMode) }}</td>
                <td>{{ statusLabel(s.status) }}</td>
                <td>{{ s.participantsCount }}</td>
                <td>
                  <a [routerLink]="['/game-sessions', s.id, 'control']"><button type="button">Control</button></a>
                </td>
              </tr>
            }
          </tbody>
        </table>
      </div>

      <div class="mobile-list">
        @for (s of sessions; track s.id) {
          <article class="mobile-item">
            <div class="mobile-item-head">
              <strong>{{ s.quizTitle }}</strong>
              <span class="mobile-pill">{{ statusLabel(s.status) }}</span>
            </div>

            <div class="mobile-metrics">
              <div class="mobile-metric">
                <span>Code</span>
                <strong>{{ s.joinCode }}</strong>
              </div>
              <div class="mobile-metric">
                <span>Flow</span>
                <strong>{{ flowModeLabel(s.questionFlowMode) }}</strong>
              </div>
              <div class="mobile-metric">
                <span>Participants</span>
                <strong>{{ s.participantsCount }}</strong>
              </div>
            </div>

            <a [routerLink]="['/game-sessions', s.id, 'control']" class="mobile-link">
              <button type="button">Control</button>
            </a>
          </article>
        }
      </div>
    </div>
  `,
  styles: [`
    :host {
      display: block;
      min-width: 0;
      max-width: 100%;
    }

    .list-shell {
      display: grid;
      gap: 14px;
      min-width: 0;
    }

    .list-shell h2 {
      margin: 0;
    }

    .create-bar {
      display: grid;
      grid-template-columns: minmax(0, 1.3fr) minmax(0, 1fr) auto;
      gap: 10px;
      min-width: 0;
      align-items: end;
    }

    .create-bar select {
      min-width: 0;
    }

    .create-bar button {
      min-height: 42px;
    }

    .desktop-table {
      display: block;
    }

    .mobile-list {
      display: none;
      gap: 10px;
    }

    .mobile-item {
      display: grid;
      gap: 12px;
      padding: 14px;
      border-radius: 18px;
      border: 1px solid var(--border);
      background: var(--surface-soft);
    }

    .mobile-item-head {
      display: flex;
      justify-content: space-between;
      gap: 10px;
      align-items: flex-start;
    }

    .mobile-item-head strong {
      min-width: 0;
      color: var(--text);
      line-height: 1.4;
    }

    .mobile-pill {
      flex-shrink: 0;
      display: inline-flex;
      align-items: center;
      justify-content: center;
      padding: 6px 10px;
      border-radius: 999px;
      border: 1px solid var(--border);
      background: var(--surface);
      color: var(--text-soft);
      font-size: 0.78rem;
      font-weight: 700;
    }

    .mobile-metrics {
      display: grid;
      grid-template-columns: repeat(3, minmax(0, 1fr));
      gap: 8px;
    }

    .mobile-metric {
      min-width: 0;
      padding: 10px 12px;
      border-radius: 14px;
      border: 1px solid var(--border);
      background: var(--surface);
    }

    .mobile-metric span {
      display: block;
      margin-bottom: 4px;
      color: var(--muted);
      font-size: 0.72rem;
      font-weight: 700;
      letter-spacing: 0.08em;
      text-transform: uppercase;
    }

    .mobile-metric strong {
      color: var(--text);
      line-height: 1.4;
    }

    .mobile-link {
      text-decoration: none;
    }

    .mobile-link button {
      width: 100%;
    }

    @media (max-width: 900px) {
      .create-bar {
        grid-template-columns: 1fr;
      }
    }

    @media (max-width: 760px) {
      .desktop-table {
        display: none;
      }

      .mobile-list {
        display: grid;
      }
    }

    @media (max-width: 520px) {
      .mobile-item {
        padding: 12px;
      }

      .mobile-item-head {
        flex-direction: column;
        align-items: stretch;
      }

      .mobile-pill {
        width: fit-content;
      }

      .mobile-metrics {
        grid-template-columns: 1fr;
      }
    }
  `]
})
export class GameSessionsListComponent implements OnInit, OnDestroy {
  quizzes: any[] = [];
  sessions: any[] = [];
  quizId = 0;
  questionFlowMode = 1;
  error = '';

  constructor(
    private quizService: QuizService,
    private sessionService: GameSessionService,
    private signalr: SignalrService
  ) {}

  ngOnInit(): void {
    this.quizService.getAll({ mode: 2, pageNumber: 1, pageSize: 200 }).subscribe((res: any) => this.quizzes = res.items || []);
    this.load();
    this.initRealtime();
  }

  ngOnDestroy(): void {
    this.signalr.off('sessionsUpdated');
    this.signalr.off('sessionDeleted');
    this.signalr.leaveGlobalSessionsGroup();
    this.signalr.disconnect();
  }

  load(): void {
    this.sessionService.getAll().subscribe({
      next: (res) => this.sessions = res,
      error: (err) => this.error = err?.error?.message || 'Failed to load sessions'
    });
  }

  create(): void {
    if (!this.quizId) return;
    this.sessionService.create({ quizId: this.quizId, questionFlowMode: this.questionFlowMode }).subscribe({
      next: () => this.load(),
      error: (err) => this.error = err?.error?.message || 'Failed to create session'
    });
  }

  statusLabel(v: number): string {
    return ['', 'Draft', 'Waiting', 'Live', 'Paused', 'Ended'][v] || 'Unknown';
  }

  flowModeLabel(v: number): string {
    return Number(v) === 2 ? 'Timed' : 'Host';
  }

  private async initRealtime(): Promise<void> {
    try {
      await this.signalr.connect();
      await this.signalr.joinGlobalSessionsGroup();

      this.signalr.on('sessionsUpdated', (payload: any) => {
        this.upsertSession(payload);
      });
      this.signalr.on('sessionDeleted', (payload: any) => {
        this.removeSession(payload);
      });
    } catch {
      this.error = 'Live updates unavailable. Refresh manually.';
    }
  }

  private upsertSession(payload: any): void {
    if (!payload) return;

    const id = Number(payload.id ?? payload.Id ?? 0);
    if (!id) {
      this.load();
      return;
    }

    const normalized = {
      id,
      quizId: Number(payload.quizId ?? payload.QuizId ?? 0),
      quizTitle: payload.quizTitle ?? payload.QuizTitle ?? '',
      hostId: payload.hostId ?? payload.HostId ?? null,
      joinCode: payload.joinCode ?? payload.JoinCode ?? '',
      joinLink: payload.joinLink ?? payload.JoinLink ?? '',
      status: Number(payload.status ?? payload.Status ?? 0),
      questionFlowMode: Number(payload.questionFlowMode ?? payload.QuestionFlowMode ?? 1),
      currentQuestionIndex: Number(payload.currentQuestionIndex ?? payload.CurrentQuestionIndex ?? 0),
      startedAt: payload.startedAt ?? payload.StartedAt ?? null,
      endedAt: payload.endedAt ?? payload.EndedAt ?? null,
      createdAt: payload.createdAt ?? payload.CreatedAt ?? null,
      participantsCount: Number(payload.participantsCount ?? payload.ParticipantsCount ?? 0)
    };

    const index = this.sessions.findIndex((x) => Number(x.id) === id);
    if (index >= 0) {
      this.sessions[index] = { ...this.sessions[index], ...normalized };
      this.sessions = [...this.sessions];
      return;
    }

    this.sessions = [normalized, ...this.sessions];
  }

  private removeSession(payload: any): void {
    const id = Number(payload?.id ?? payload?.sessionId ?? payload?.Id ?? 0);
    if (!id) return;
    this.sessions = this.sessions.filter((x) => Number(x?.id) !== id);
  }
}
