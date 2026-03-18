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
        <div>
          <p class="eyebrow">Test Library</p>
          <h2>Tests</h2>
        </div>
        <a routerLink="/quizzes/new"><button type="button">New Test</button></a>
      </div>

      <div class="filters-grid">
        <div class="field">
          <label for="tests-search">Search</label>
          <input id="tests-search" name="testsSearch" [(ngModel)]="search" placeholder="Search title or description" (keyup.enter)="load()" />
        </div>

        <div class="field">
          <label for="tests-category">Category</label>
          <input id="tests-category" name="testsCategory" [(ngModel)]="category" placeholder="Unit 1" (keyup.enter)="load()" />
        </div>

        <div class="field">
          <label for="tests-status">Status</label>
          <select id="tests-status" name="testsStatus" [(ngModel)]="published" (change)="load()">
            <option [ngValue]="null">All</option>
            <option [ngValue]="true">Published</option>
            <option [ngValue]="false">Draft</option>
          </select>
        </div>

        <button type="button" class="filter-button" (click)="load()">Filter</button>
      </div>

      @if (error) { <div class="alert">{{ error }}</div> }

      <div class="table-wrap desktop-table">
        <table>
          <thead>
            <tr><th>Test</th><th>Categories</th><th>Questions</th><th>Marks</th><th>Status</th><th>Actions</th></tr>
          </thead>
          <tbody>
            @for (quiz of items; track quiz.id) {
              <tr>
                <td>
                  <strong>{{ quiz.title }}</strong>
                  @if (quiz.description) {
                    <div class="cell-copy">{{ quiz.description }}</div>
                  }
                </td>
                <td>{{ categoryLabel(quiz) }}</td>
                <td>{{ quiz.questionsCount }}</td>
                <td>{{ marksLabel(quiz) }}</td>
                <td>{{ quiz.isPublished ? 'Published' : 'Draft' }}</td>
                <td class="table-actions">
                  <a [routerLink]="['/quizzes', quiz.id]"><button type="button" class="secondary">Builder</button></a>
                  <a [routerLink]="['/quizzes', quiz.id, 'edit']"><button type="button" class="secondary">Edit</button></a>
                  <button type="button" (click)="togglePublish(quiz)">{{ quiz.isPublished ? 'Unpublish' : 'Publish' }}</button>
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
            <div class="mobile-head">
              <strong>{{ quiz.title }}</strong>
              <span class="mobile-pill">{{ quiz.isPublished ? 'Published' : 'Draft' }}</span>
            </div>

            @if (quiz.description) {
              <p class="mobile-copy">{{ quiz.description }}</p>
            }

            <div class="mobile-metrics">
              <div class="mobile-metric">
                <span>Categories</span>
                <strong>{{ categoryLabel(quiz) }}</strong>
              </div>
              <div class="mobile-metric">
                <span>Questions</span>
                <strong>{{ quiz.questionsCount }}</strong>
              </div>
              <div class="mobile-metric">
                <span>Marks</span>
                <strong>{{ marksLabel(quiz) }}</strong>
              </div>
            </div>

            <div class="mobile-actions">
              <a [routerLink]="['/quizzes', quiz.id]"><button type="button" class="secondary">Builder</button></a>
              <a [routerLink]="['/quizzes', quiz.id, 'edit']"><button type="button" class="secondary">Edit</button></a>
              <button type="button" (click)="togglePublish(quiz)">{{ quiz.isPublished ? 'Unpublish' : 'Publish' }}</button>
              <button type="button" class="danger" (click)="remove(quiz.id)">Delete</button>
            </div>
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

    .filters-grid {
      display: grid;
      grid-template-columns: minmax(0, 1.3fr) minmax(0, 1fr) minmax(160px, 220px) auto;
      gap: 10px;
      align-items: end;
    }

    .field {
      display: grid;
      gap: 6px;
      min-width: 0;
    }

    .filter-button {
      min-height: 42px;
    }

    .cell-copy,
    .mobile-copy {
      margin-top: 6px;
      color: var(--muted-strong);
      line-height: 1.45;
    }

    .table-actions {
      display: flex;
      flex-wrap: wrap;
      gap: 8px;
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

    .mobile-actions {
      display: grid;
      grid-template-columns: repeat(2, minmax(0, 1fr));
      gap: 8px;
    }

    .mobile-actions a,
    .mobile-actions button {
      width: 100%;
    }

    @media (max-width: 900px) {
      .filters-grid {
        grid-template-columns: repeat(2, minmax(0, 1fr));
      }
    }

    @media (max-width: 760px) {
      .filters-grid {
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
      .mobile-head {
        flex-direction: column;
        align-items: stretch;
      }

      .mobile-pill {
        width: fit-content;
      }

      .mobile-metrics,
      .mobile-actions {
        grid-template-columns: 1fr;
      }
    }
  `]
})
export class QuizzesListComponent implements OnInit {
  items: any[] = [];
  search = '';
  category = '';
  published: boolean | null = null;
  error = '';

  constructor(private service: QuizService, private confirmDialog: ConfirmDialogService) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.error = '';
    this.service.getAll({ search: this.search, category: this.category, isPublished: this.published, mode: 1 }).subscribe({
      next: (res: any) => this.items = res.items || [],
      error: (err) => this.error = err?.error?.message || 'Failed to load tests'
    });
  }

  togglePublish(quiz: any): void {
    this.service.publish(quiz.id, !quiz.isPublished).subscribe({
      next: () => this.load(),
      error: (err) => this.error = err?.error?.message || 'Status update failed'
    });
  }

  async remove(id: number): Promise<void> {
    const ok = await this.confirmDialog.open({
      title: 'Delete test?',
      message: 'This test and its builder configuration will be removed from the system.',
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

  categoryLabel(item: any): string {
    const categories = Array.isArray(item?.categories) ? item.categories.map((category: any) => category?.name).filter(Boolean) : [];
    return categories.length ? categories.join(', ') : 'Uncategorized';
  }

  marksLabel(item: any): string {
    const totalMarks = Number(item?.totalMarks ?? 0);
    const effectiveMarks = Number(item?.effectiveTotalMarks ?? 0);
    if (totalMarks > 0) {
      return `${totalMarks} total`;
    }

    return effectiveMarks > 0 ? `${effectiveMarks} auto` : '-';
  }
}
