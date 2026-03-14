import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { QuizService } from '../../core/services/quiz.service';
import { ConfirmDialogService } from '../../core/services/confirm-dialog.service';

@Component({
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  template: `
    <div class="card list-shell">
      <div class="list-head">
        <h2>Quizzes</h2>
        <a routerLink="/quizzes/new"><button type="button">New Quiz</button></a>
      </div>

      <div class="list-filters filters-wide">
        <div class="col filter-field">
          <label for="quizzes-search">Search</label>
          <input id="quizzes-search" name="quizzesSearch" [(ngModel)]="search" placeholder="Search" (keyup.enter)="load()" />
        </div>

        <div class="col filter-field">
          <label for="quizzes-mode">Mode</label>
          <select id="quizzes-mode" name="quizzesMode" [(ngModel)]="mode" (change)="load()">
            <option [ngValue]="null">All Modes</option>
            <option [ngValue]="1">Test</option>
            <option [ngValue]="2">Game</option>
          </select>
        </div>

        <div class="col filter-field">
          <label for="quizzes-published">Status</label>
          <select id="quizzes-published" name="quizzesPublished" [(ngModel)]="published" (change)="load()">
            <option [ngValue]="null">All</option>
            <option [ngValue]="true">Published</option>
            <option [ngValue]="false">Unpublished</option>
          </select>
        </div>

        <button type="button" class="filter-button" (click)="load()">Filter</button>
      </div>

      @if (error) { <div class="alert">{{ error }}</div> }

      <div class="table-wrap desktop-table">
        <table>
          <thead>
            <tr><th>Title</th><th>Mode</th><th>Questions</th><th>Status</th><th>Actions</th></tr>
          </thead>
          <tbody>
            @for (quiz of items; track quiz.id) {
              <tr>
                <td>{{ displayTitle(quiz) }}</td>
                <td>{{ modeLabel(quiz.mode) }}</td>
                <td>{{ questionsCount(quiz) }}</td>
                <td>{{ isPublished(quiz) ? 'Published' : 'Draft' }}</td>
                <td class="row table-actions">
                  <a [routerLink]="['/quizzes', quiz.id]"><button type="button" class="secondary">Details</button></a>
                  <a [routerLink]="['/quizzes', quiz.id, 'edit']"><button type="button" class="secondary">Edit</button></a>
                  <button type="button" (click)="togglePublish(quiz)">{{ isPublished(quiz) ? 'Unpublish' : 'Publish' }}</button>
                  <button type="button" class="danger" (click)="remove(quiz.id)">Delete</button>
                </td>
              </tr>
            }
          </tbody>
        </table>
      </div>

      <div class="mobile-list">
        @for (quiz of items; track quiz.id) {
          <article class="mobile-item">
            <div class="mobile-item-head">
              <strong>{{ displayTitle(quiz) }}</strong>
              <span class="mobile-pill">{{ isPublished(quiz) ? 'Published' : 'Draft' }}</span>
            </div>

            <div class="mobile-metrics">
              <div class="mobile-metric">
                <span>Mode</span>
                <strong>{{ modeLabel(quiz.mode) }}</strong>
              </div>
              <div class="mobile-metric">
                <span>Questions</span>
                <strong>{{ questionsCount(quiz) }}</strong>
              </div>
            </div>

            <div class="mobile-actions mobile-actions-wide">
              <a [routerLink]="['/quizzes', quiz.id]"><button type="button" class="secondary">Details</button></a>
              <a [routerLink]="['/quizzes', quiz.id, 'edit']"><button type="button" class="secondary">Edit</button></a>
              <button type="button" (click)="togglePublish(quiz)">{{ isPublished(quiz) ? 'Unpublish' : 'Publish' }}</button>
              <button type="button" class="danger" (click)="remove(quiz.id)">Delete</button>
            </div>
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

    .list-head {
      display: flex;
      align-items: flex-start;
      justify-content: space-between;
      gap: 10px;
      flex-wrap: wrap;
    }

    .list-head h2 {
      margin: 0;
    }

    .list-head a,
    .mobile-actions a,
    .table-actions a {
      text-decoration: none;
    }

    .list-filters {
      display: grid;
      gap: 10px;
      align-items: end;
      min-width: 0;
    }

    .filters-wide {
      grid-template-columns: minmax(0, 1fr) minmax(150px, 180px) minmax(150px, 180px) auto;
    }

    .filter-field {
      min-width: 0;
    }

    .filter-button {
      min-height: 42px;
    }

    .desktop-table {
      display: block;
    }

    .table-actions {
      gap: 8px;
      align-items: center;
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
      grid-template-columns: repeat(2, minmax(0, 1fr));
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

    .mobile-actions {
      display: grid;
      gap: 8px;
    }

    .mobile-actions-wide {
      grid-template-columns: repeat(2, minmax(0, 1fr));
    }

    .mobile-actions a,
    .mobile-actions button {
      width: 100%;
    }

    @media (max-width: 820px) {
      .filters-wide {
        grid-template-columns: repeat(2, minmax(0, 1fr));
      }
    }

    @media (max-width: 760px) {
      .list-shell {
        gap: 12px;
      }

      .filters-wide {
        grid-template-columns: 1fr;
      }

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

      .mobile-metrics,
      .mobile-actions-wide {
        grid-template-columns: 1fr;
      }
    }
  `]
})
export class QuizzesListComponent implements OnInit {
  items: any[] = [];
  search = '';
  mode: number | null = null;
  published: boolean | null = null;
  error = '';

  constructor(private service: QuizService, private confirmDialog: ConfirmDialogService) {}

  ngOnInit(): void { this.load(); }

  load(): void {
    this.service.getAll({ search: this.search, mode: this.mode, isPublished: this.published }).subscribe({
      next: (res: any) => this.items = res.items || [],
      error: (err) => this.error = err?.error?.message || 'Failed to load quizzes'
    });
  }

  togglePublish(quiz: any): void {
    this.service.publish(quiz.id, !this.isPublished(quiz)).subscribe({
      next: () => this.load(),
      error: (err) => this.error = err?.error?.message || 'Publish update failed'
    });
  }

  async remove(id: number): Promise<void> {
    const ok = await this.confirmDialog.open({
      title: 'Delete quiz?',
      message: 'This quiz will be removed permanently from the system.',
      confirmText: 'Delete',
      cancelText: 'Keep it',
      tone: 'danger'
    });

    if (!ok) return;

    this.service.delete(id).subscribe({
      next: () => this.load(),
      error: (err) => this.error = err?.error?.message || 'Delete failed'
    });
  }

  displayTitle(item: any): string {
    return item?.title ?? item?.quizTitle ?? `#${item?.id ?? ''}`;
  }

  modeLabel(mode: any): string {
    if (mode === 'Test' || mode === 'Game') return mode;
    return Number(mode) === 1 ? 'Test' : 'Game';
  }

  questionsCount(item: any): number {
    return Number(item?.questionsCount ?? item?.questions?.length ?? 0);
  }

  isPublished(item: any): boolean {
    return Boolean(item?.isPublished ?? item?.published ?? false);
  }
}
