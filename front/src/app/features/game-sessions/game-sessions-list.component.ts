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
      <div class="list-head">
        <div>
          <p class="eyebrow">Live Delivery</p>
          <h2>Sessions</h2>
          <p class="intro-copy">Create a live session from any reusable test, then control it in real time.</p>
        </div>
      </div>

      <section class="session-builder">
        <div class="builder-grid">
          <div class="field span-2">
            <label for="session-search">Search tests or categories</label>
            <input id="session-search" name="sessionSearch" [(ngModel)]="testSearch" placeholder="Search Unit 1, Biology, Midterm..." />
          </div>

          <div class="field">
            <label for="session-category">Category filter</label>
            <select id="session-category" name="sessionCategory" [(ngModel)]="categoryFilter">
              <option value="">All categories</option>
              @for (category of categories; track category.id) {
                <option [value]="category.name">{{ category.name }}</option>
              }
            </select>
          </div>

          <div class="field">
            <label for="session-test">Linked test</label>
            <select id="session-test" name="sessionTest" [(ngModel)]="quizId">
              <option [ngValue]="0">Select a test</option>
              @for (quiz of filteredTests(); track quiz.id) {
                <option [ngValue]="quiz.id">{{ quiz.title }} | {{ sourceCategoryLabel(quiz) }}</option>
              }
            </select>
          </div>

          <div class="field">
            <label for="session-access">Access</label>
            <select id="session-access" name="sessionAccess" [(ngModel)]="accessType">
              <option [ngValue]="2">Private</option>
              <option [ngValue]="1">Public</option>
            </select>
          </div>

          <div class="field">
            <label for="session-flow">Question flow</label>
            <select id="session-flow" name="sessionFlow" [(ngModel)]="questionFlowMode">
              <option [ngValue]="1">Host controlled</option>
              <option [ngValue]="2">Timed by question</option>
            </select>
          </div>

          <div class="field">
            <label for="session-duration">Duration (minutes)</label>
            <input id="session-duration" name="sessionDuration" type="number" [(ngModel)]="durationMinutes" min="0" placeholder="Use test duration if empty" />
          </div>

          <div class="field">
            <label for="session-start">Start time</label>
            <input id="session-start" name="sessionStart" type="datetime-local" [(ngModel)]="scheduledStartAt" />
          </div>

          <div class="field">
            <label for="session-end">End time</label>
            <input id="session-end" name="sessionEnd" type="datetime-local" [(ngModel)]="scheduledEndAt" />
          </div>
        </div>

        <div class="builder-actions">
          <button type="button" (click)="create()">Create Session</button>
        </div>
      </section>

      @if (error) { <div class="alert">{{ error }}</div> }

      <div class="table-wrap desktop-table">
        <table>
          <thead>
            <tr><th>Test</th><th>Categories</th><th>Schedule</th><th>Access</th><th>Status</th><th>Participants</th><th>Actions</th></tr>
          </thead>
          <tbody>
            @for (session of sessions; track session.id) {
              <tr>
                <td>{{ session.quizTitle }}</td>
                <td>{{ sourceCategoryLabel(session) }}</td>
                <td>{{ scheduleLabel(session) }}</td>
                <td>{{ accessLabel(session.accessType) }}</td>
                <td>{{ statusLabel(session.status) }}</td>
                <td>{{ session.participantsCount }}</td>
                <td>
                  <a [routerLink]="['/game-sessions', session.id, 'control']"><button type="button">Control</button></a>
                </td>
              </tr>
            }
          </tbody>
        </table>
      </div>

      <div class="mobile-list">
        @for (session of sessions; track session.id) {
          <article class="mobile-item">
            <div class="mobile-head">
              <strong>{{ session.quizTitle }}</strong>
              <span class="mobile-pill">{{ statusLabel(session.status) }}</span>
            </div>

            <div class="mobile-copy">{{ sourceCategoryLabel(session) }}</div>

            <div class="mobile-metrics">
              <div class="mobile-metric">
                <span>Schedule</span>
                <strong>{{ scheduleLabel(session) }}</strong>
              </div>
              <div class="mobile-metric">
                <span>Access</span>
                <strong>{{ accessLabel(session.accessType) }}</strong>
              </div>
              <div class="mobile-metric">
                <span>Players</span>
                <strong>{{ session.participantsCount }}</strong>
              </div>
            </div>

            <a [routerLink]="['/game-sessions', session.id, 'control']">
              <button type="button">Control</button>
            </a>
          </article>
        }
      </div>
    </div>
  `,
  styles: [`
    .list-shell {
      display: grid;
      gap: 14px;
    }

    .list-head h2 {
      margin: 0;
    }

    .intro-copy {
      margin: 6px 0 0;
      max-width: 58ch;
    }

    .session-builder {
      display: grid;
      gap: 12px;
      padding: 16px;
      border-radius: 20px;
      border: 1px solid var(--border);
      background: var(--surface-soft);
    }

    .builder-grid {
      display: grid;
      gap: 10px;
      grid-template-columns: repeat(2, minmax(0, 1fr));
    }

    .field {
      display: grid;
      gap: 6px;
      min-width: 0;
    }

    .span-2 {
      grid-column: span 2;
    }

    .builder-actions {
      display: flex;
      justify-content: flex-end;
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

    .mobile-item a {
      text-decoration: none;
    }

    .mobile-head {
      display: flex;
      justify-content: space-between;
      gap: 10px;
      align-items: flex-start;
    }

    .mobile-pill {
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

    .mobile-copy {
      color: var(--muted-strong);
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

    @media (max-width: 900px) {
      .builder-grid {
        grid-template-columns: 1fr;
      }

      .span-2 {
        grid-column: auto;
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
      .mobile-head {
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
  categories: any[] = [];
  sessions: any[] = [];
  quizId = 0;
  accessType = 2;
  questionFlowMode = 1;
  durationMinutes: number | null = null;
  scheduledStartAt = '';
  scheduledEndAt = '';
  testSearch = '';
  categoryFilter = '';
  error = '';

  constructor(
    private quizService: QuizService,
    private sessionService: GameSessionService,
    private signalr: SignalrService
  ) {}

  ngOnInit(): void {
    this.quizService.getAll({ mode: 1, pageNumber: 1, pageSize: 200 }).subscribe((res: any) => this.quizzes = res.items || []);
    this.quizService.getCategories().subscribe((items) => this.categories = items);
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

  filteredTests(): any[] {
    const search = String(this.testSearch || '').trim().toLowerCase();
    const category = String(this.categoryFilter || '').trim().toLowerCase();

    return this.quizzes.filter((quiz) => {
      const matchesSearch = !search
        || String(quiz?.title || '').toLowerCase().includes(search)
        || this.sourceCategoryLabel(quiz).toLowerCase().includes(search);
      const matchesCategory = !category || this.sourceCategoryLabel(quiz).toLowerCase().includes(category);
      return matchesSearch && matchesCategory;
    });
  }

  create(): void {
    if (!this.quizId) {
      this.error = 'Select a test first.';
      return;
    }

    this.error = '';
    this.sessionService.create({
      quizId: this.quizId,
      questionFlowMode: this.questionFlowMode,
      accessType: this.accessType,
      durationMinutes: this.normalizeOptionalNumber(this.durationMinutes),
      scheduledStartAt: this.toUtcIso(this.scheduledStartAt),
      scheduledEndAt: this.toUtcIso(this.scheduledEndAt)
    }).subscribe({
      next: () => {
        this.quizId = 0;
        this.durationMinutes = null;
        this.scheduledStartAt = '';
        this.scheduledEndAt = '';
        this.load();
      },
      error: (err) => this.error = err?.error?.message || 'Failed to create session'
    });
  }

  statusLabel(v: number): string {
    return ['', 'Draft', 'Waiting', 'Live', 'Paused', 'Ended'][v] || 'Unknown';
  }

  accessLabel(v: number): string {
    return Number(v) === 1 ? 'Public' : 'Private';
  }

  scheduleLabel(session: any): string {
    const start = session?.scheduledStartAt ? new Date(session.scheduledStartAt) : null;
    const end = session?.scheduledEndAt ? new Date(session.scheduledEndAt) : null;

    if (start && end) {
      return `${start.toLocaleString()} to ${end.toLocaleString()}`;
    }

    if (start) {
      return `Starts ${start.toLocaleString()}`;
    }

    if (session?.durationMinutes) {
      return `${session.durationMinutes} min`;
    }

    return 'On demand';
  }

  sourceCategoryLabel(item: any): string {
    const categories = Array.isArray(item?.categories) ? item.categories.map((category: any) => category?.name).filter(Boolean) : [];
    return categories.length ? categories.join(', ') : 'Uncategorized';
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
      quizCoverImageUrl: payload.quizCoverImageUrl ?? payload.QuizCoverImageUrl ?? '',
      hostId: payload.hostId ?? payload.HostId ?? null,
      joinCode: payload.joinCode ?? payload.JoinCode ?? '',
      joinLink: payload.joinLink ?? payload.JoinLink ?? '',
      status: Number(payload.status ?? payload.Status ?? 0),
      accessType: Number(payload.accessType ?? payload.AccessType ?? 2),
      questionFlowMode: Number(payload.questionFlowMode ?? payload.QuestionFlowMode ?? 1),
      scheduledStartAt: payload.scheduledStartAt ?? payload.ScheduledStartAt ?? null,
      scheduledEndAt: payload.scheduledEndAt ?? payload.ScheduledEndAt ?? null,
      durationMinutes: payload.durationMinutes ?? payload.DurationMinutes ?? null,
      currentQuestionIndex: Number(payload.currentQuestionIndex ?? payload.CurrentQuestionIndex ?? 0),
      startedAt: payload.startedAt ?? payload.StartedAt ?? null,
      endedAt: payload.endedAt ?? payload.EndedAt ?? null,
      createdAt: payload.createdAt ?? payload.CreatedAt ?? null,
      participantsCount: Number(payload.participantsCount ?? payload.ParticipantsCount ?? 0),
      categories: Array.isArray(payload?.categories ?? payload?.Categories)
        ? (payload.categories ?? payload.Categories).map((category: any) => ({
            id: Number(category?.id ?? category?.Id ?? 0),
            name: String(category?.name ?? category?.Name ?? '')
          }))
        : []
    };

    const index = this.sessions.findIndex((item) => Number(item.id) === id);
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
    this.sessions = this.sessions.filter((item) => Number(item?.id) !== id);
  }

  private toUtcIso(value: string): string | null {
    const trimmed = String(value || '').trim();
    if (!trimmed) return null;

    const date = new Date(trimmed);
    return Number.isNaN(date.getTime()) ? null : date.toISOString();
  }

  private normalizeOptionalNumber(value: any): number | null {
    if (value === null || value === undefined || value === '') return null;
    const parsed = Number(value);
    if (!Number.isFinite(parsed) || parsed <= 0) return null;
    return Math.floor(parsed);
  }
}
