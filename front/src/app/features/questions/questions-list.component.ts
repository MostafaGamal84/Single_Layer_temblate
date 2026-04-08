import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { QuestionService } from '../../core/services/question.service';
import { ConfirmDialogService } from '../../core/services/confirm-dialog.service';
import { ToastService } from '../../core/services/toast.service';

@Component({
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  template: `
    <div class="card list-shell">
      <div class="list-head">
        <h2>Questions</h2>
        <a routerLink="/questions/new"><button type="button">New Question</button></a>
      </div>

      <div class="list-filters">
        <div class="col filter-field">
          <label for="questions-search">Search</label>
          <input id="questions-search" name="questionsSearch" [(ngModel)]="search" placeholder="Search..." (keyup.enter)="load()" />
        </div>

        <div class="col filter-field filter-field-type">
          <label for="questions-type">Question type</label>
          <select id="questions-type" name="questionsType" [(ngModel)]="type" (change)="load()">
            <option [ngValue]="null">All Types</option>
            <option [ngValue]="1">Multiple Choice</option>
            <option [ngValue]="2">True / False</option>
            <option [ngValue]="3">Short Answer</option>
          </select>
        </div>

        <button type="button" class="filter-button" (click)="load()">Filter</button>
      </div>

      @if (error) { <div class="alert">{{ error }}</div> }

      <div class="table-wrap desktop-table">
        <table>
          <thead>
            <tr><th>Title</th><th>Format</th><th>Points</th><th>Answer Time</th><th>Actions</th></tr>
          </thead>
          <tbody>
            @for (q of items; track q.id) {
              <tr>
                <td>{{ displayTitle(q) }}</td>
                <td>{{ typeLabel(q) }}</td>
                <td>{{ displayPoints(q) }}</td>
                <td>{{ displayAnswerSeconds(q) }}s</td>
                <td class="row table-actions">
                  <a [routerLink]="['/questions', q.id, 'edit']"><button type="button" class="secondary">Edit</button></a>
                  <button type="button" class="danger" (click)="remove(q.id)">Delete</button>
                </td>
              </tr>
            }
          </tbody>
        </table>
      </div>

      <div class="mobile-list">
        @for (q of items; track q.id) {
          <article class="mobile-item">
            <div class="mobile-item-head">
              <strong>{{ displayTitle(q) }}</strong>
              <span class="mobile-pill">{{ typeLabel(q) }}</span>
            </div>

            <div class="mobile-metrics">
              <div class="mobile-metric">
                <span>Points</span>
                <strong>{{ displayPoints(q) }}</strong>
              </div>
              <div class="mobile-metric">
                <span>Answer Time</span>
                <strong>{{ displayAnswerSeconds(q) }}s</strong>
              </div>
            </div>

            <div class="mobile-actions">
              <a [routerLink]="['/questions', q.id, 'edit']"><button type="button" class="secondary">Edit</button></a>
              <button type="button" class="danger" (click)="remove(q.id)">Delete</button>
            </div>
          </article>
        }
      </div>

      <div class="pager row">
        <button type="button" class="secondary" [disabled]="page <= 1" (click)="page = page - 1; load()">Prev</button>
        <span>Page {{ page }}</span>
        <button type="button" class="secondary" [disabled]="page * size >= total" (click)="page = page + 1; load()">Next</button>
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
      grid-template-columns: minmax(0, 1fr) minmax(180px, 220px) auto;
      gap: 10px;
      align-items: end;
      min-width: 0;
    }

    .filter-field {
      min-width: 0;
    }

    .filter-button {
      min-height: 42px;
    }

    .desktop-table {
      display: block;
      min-height: 400px;
    }

    .table-actions {
      gap: 8px;
      align-items: center;
    }

    .mobile-list {
      display: none;
      gap: 10px;
      min-height: 300px;
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
      grid-template-columns: repeat(2, minmax(0, 1fr));
      gap: 8px;
    }

    .mobile-actions a,
    .mobile-actions button {
      width: 100%;
    }

    .pager {
      justify-content: flex-end;
      margin-top: 2px;
    }

    @media (max-width: 760px) {
      .list-shell {
        gap: 12px;
      }

      .list-filters {
        grid-template-columns: 1fr;
      }

      .desktop-table {
        display: none;
      }

      .mobile-list {
        display: grid;
      }

      .pager {
        justify-content: stretch;
      }

      .pager button {
        flex: 1 1 0;
      }
    }

    @media (max-width: 520px) {
      .mobile-item {
        padding: 12px;
      }

      .mobile-item-head,
      .mobile-actions,
      .mobile-metrics {
        grid-template-columns: 1fr;
      }

      .mobile-item-head {
        flex-direction: column;
        align-items: stretch;
      }

      .mobile-pill {
        width: fit-content;
      }
    }
  `]
})
export class QuestionsListComponent implements OnInit {
  items: any[] = [];
  search = '';
  type: number | null = null;
  page = 1;
  size = 10;
  total = 0;
  error = '';

  constructor(
    private service: QuestionService,
    private confirmDialog: ConfirmDialogService,
    private toast: ToastService
  ) {}

  ngOnInit(): void { this.load(); }

  load(): void {
    this.error = '';
    this.service.getAll({ pageNumber: this.page, pageSize: this.size, search: this.search, type: this.type }).subscribe({
      next: (res: any) => {
        this.items = res.items || [];
        this.total = res.totalCount || 0;
        this.items = this.items.filter((x) => x && x.id > 0);
      },
      error: (err) => this.error = err?.error?.message || 'Failed to load questions'
    });
  }

  async remove(id: number): Promise<void> {
    const ok = await this.confirmDialog.open({
      title: 'Delete question?',
      message: 'This question will be removed from the bank and cannot be restored.',
      confirmText: 'Delete',
      cancelText: 'Cancel',
      tone: 'danger'
    });

    if (!ok) return;

    this.service.delete(id).subscribe({
      next: () => {
        this.load();
        this.toast.success('Question deleted successfully');
      },
      error: (err) => {
        this.error = err?.error?.message || 'Delete failed';
        this.toast.error(this.error);
      }
    });
  }

  typeLabel(item: any): string {
    const normalizedType = Number(item?.type ?? 0);
    const selectionMode = Number(item?.selectionMode ?? 1);

    if (normalizedType === 3) return 'Short Answer';
    if (normalizedType === 2) return 'True / False';
    return selectionMode === 2 ? 'Multiple Choice' : 'Single Choice';
  }

  displayTitle(item: any): string {
    return item?.title ?? item?.questionTitle ?? item?.text ?? `#${item?.id ?? ''}`;
  }

  displayPoints(item: any): string {
    const points = item?.points ?? item?.pointsOverride;
    return points === null || points === undefined ? '-' : String(points);
  }

  displayAnswerSeconds(item: any): number {
    const value = Number(item?.answerSeconds ?? 30);
    if (!Number.isFinite(value)) return 30;
    if (value < 5) return 5;
    if (value > 300) return 300;
    return Math.floor(value);
  }
}
