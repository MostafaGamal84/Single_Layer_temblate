import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { QuizService } from '../../core/services/quiz.service';
import { ConfirmDialogService } from '../../core/services/confirm-dialog.service';
import { ToastService } from '../../core/services/toast.service';

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
        <div class="head-actions">
          <label class="import-btn">
            <input type="file" accept=".xlsx,.xls" (change)="onFileSelected($event)" hidden />
            <span>Import Excel</span>
          </label>
          <a routerLink="/quizzes/new"><button type="button">New Test</button></a>
        </div>
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

      @if (selectedIds.size > 0) {
        <div class="bulk-actions-bar">
          <span class="selection-count">{{ selectedIds.size }} selected</span>
          <div class="bulk-actions-desktop">
            <button type="button" class="secondary" (click)="clearSelection()">Clear</button>
            <button type="button" (click)="bulkExport()">Export</button>
            <button type="button" (click)="bulkPublish()">Publish</button>
            <button type="button" (click)="bulkUnpublish()">Unpublish</button>
            <button type="button" class="danger" (click)="bulkDelete()">Delete</button>
          </div>
          <select class="bulk-actions-mobile" (change)="onBulkActionSelected($event)">
            <option value="">Actions</option>
            <option value="clear">Clear</option>
            <option value="export">Export</option>
            <option value="publish">Publish</option>
            <option value="unpublish">Unpublish</option>
            <option value="delete">Delete</option>
          </select>
        </div>
      }

      <div class="table-wrap desktop-table">
        <table>
          <thead>
            <tr><th style="width: 40px;"><input type="checkbox" [checked]="allSelected" (change)="toggleSelectAll()" /></th><th>Test</th><th>Categories</th><th>Questions</th><th>Marks</th><th>Status</th><th>Actions</th></tr>
          </thead>
          <tbody>
            @for (quiz of items; track quiz.id) {
              <tr [class.selected-row]="selectedIds.has(quiz.id)">
                <td><input type="checkbox" [checked]="selectedIds.has(quiz.id)" (change)="toggleSelect(quiz.id)" /></td>
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
                  <div class="row-actions-desktop">
                    <button type="button" class="secondary" (click)="exportQuiz(quiz.id)">Export</button>
                    <a [routerLink]="['/quizzes', quiz.id, 'edit']"><button type="button" class="secondary">Edit</button></a>
                    <button type="button" (click)="togglePublish(quiz)">{{ quiz.isPublished ? 'Unpublish' : 'Publish' }}</button>
                    <button type="button" class="danger" (click)="remove(quiz.id)">Delete</button>
                  </div>
                  <select class="row-actions-mobile" (change)="onRowActionSelected($event, quiz.id)">
                    <option value="">Actions</option>
                    <option value="export">Export</option>
                    <option value="edit">Edit</option>
                    <option value="publish">{{ quiz.isPublished ? 'Unpublish' : 'Publish' }}</option>
                    <option value="delete">Delete</option>
                  </select>
                </td>
              </tr>
            }
          </tbody>
        </table>
      </div>

      <div class="mobile-list">
            @for (quiz of items; track quiz.id) {
              <article class="mobile-item" [class.selected-row]="selectedIds.has(quiz.id)">
                <div class="mobile-item-checkbox">
                  <input type="checkbox" [checked]="selectedIds.has(quiz.id)" (change)="toggleSelect(quiz.id)" />
                </div>
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
              <button type="button" class="secondary" (click)="exportQuiz(quiz.id)">Export</button>
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

    .head-actions {
      display: flex;
      align-items: center;
      gap: 10px;
    }

    .import-btn {
      display: inline-flex;
      align-items: center;
      justify-content: center;
padding: 10px 16px;
      border-radius: 8px;
      border: 1px solid var(--border);
      background: var(--surface);
      color: var(--text);
      font-size: 0.9rem;
      font-weight: 500;
      cursor: pointer;
    }

    .import-btn:hover {
      background: var(--surface-soft);
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

    .bulk-actions-bar {
      display: flex;
      align-items: center;
      gap: 16px;
      padding: 14px 18px;
      background: var(--surface-elevated);
      border-radius: 12px;
      border: 1px solid var(--border);
      flex-wrap: wrap;
    }

    .selection-count {
      font-weight: 600;
      color: var(--text);
      margin-right: auto;
      white-space: nowrap;
    }

    .bulk-actions-desktop {
      display: flex;
      gap: 8px;
      flex-wrap: wrap;
    }

    .bulk-actions-mobile {
      display: none;
      min-height: 36px;
      padding: 8px 12px;
      border: 1px solid var(--border);
      border-radius: 8px;
      background: var(--surface);
      color: var(--text);
      font-size: 0.9rem;
      cursor: pointer;
    }

    .row-actions-desktop {
      display: flex;
      gap: 6px;
      flex-wrap: wrap;
    }

    .row-actions-mobile {
      display: none;
      min-height: 30px;
      padding: 6px 10px;
      border: 1px solid var(--border);
      border-radius: 6px;
      background: var(--surface);
      color: var(--text);
      font-size: 0.85rem;
      cursor: pointer;
    }

    .bulk-actions-bar button {
      min-height: 36px;
      padding: 10px 18px;
      border: 1px solid var(--border);
      cursor: pointer;
      font-weight: 500;
      font-size: 0.9rem;
      background: var(--surface);
      color: var(--text);
    }

    .bulk-actions-bar button:hover {
      background: var(--surface-elevated);
    }

    .bulk-actions-bar button:first-of-type {
      background: var(--primary);
      color: var(--primary-contrast);
      border-color: var(--primary);
    }

    .bulk-actions-bar button:first-of-type:hover {
      background: var(--primary-hover);
    }

    .bulk-actions-bar button.danger {
      background: var(--danger);
      color: white;
      border-color: var(--danger);
    }

    .bulk-actions-bar button.danger:hover {
      background: #c82333;
      border-color: #c82333;
    }

    .selected-row {
      background: var(--ring) !important;
    }

    .mobile-item-checkbox {
      margin-bottom: 8px;
    }

    .desktop-table input[type="checkbox"] {
      width: 18px;
      height: 18px;
      cursor: pointer;
    }

    .mobile-item-checkbox input[type="checkbox"] {
      width: 20px;
      height: 20px;
      cursor: pointer;
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
      .bulk-actions-desktop {
        display: none;
      }

      .bulk-actions-mobile {
        display: inline-block;
      }

      .row-actions-desktop {
        display: none;
      }

      .row-actions-mobile {
        display: inline-block;
      }

      .table-actions {
        min-width: 100px;
      }

      .desktop-table {
        display: block;
      }

      .mobile-list {
        display: none;
      }
    }

    @media (max-width: 520px) {
      .bulk-actions-bar {
        gap: 8px;
        padding: 10px 12px;
      }

      .bulk-actions-mobile {
        min-height: 32px;
        padding: 6px 10px;
        font-size: 0.85rem;
      }

      .row-actions-mobile {
        min-height: 28px;
        padding: 4px 8px;
        font-size: 0.8rem;
      }

      .selection-count {
        font-size: 0.9rem;
      }

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
  selectedFile: File | null = null;
  selectedIds = new Set<number>();
  allSelected = false;

  constructor(
    private service: QuizService,
    private confirmDialog: ConfirmDialogService,
    private toast: ToastService
  ) {}

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

  exportQuiz(id: number): void {
    this.service.exportQuiz(id).subscribe({
      next: () => this.toast.show('Quiz exported successfully', 'success'),
      error: (err) => this.toast.show(err?.error?.message || 'Export failed', 'error')
    });
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.selectedFile = input.files[0];
      this.importQuiz();
    }
  }

  importQuiz(): void {
    if (!this.selectedFile) return;

    this.service.importQuiz(this.selectedFile).subscribe({
      next: (res) => {
        this.toast.show(res.message || 'Quiz imported successfully', 'success');
        this.load();
        this.selectedFile = null;
      },
      error: (err) => this.toast.show(err?.error?.message || 'Import failed', 'error')
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

  toggleSelect(id: number): void {
    if (this.selectedIds.has(id)) {
      this.selectedIds.delete(id);
    } else {
      this.selectedIds.add(id);
    }
    this.updateAllSelected();
  }

  toggleSelectAll(): void {
    if (this.allSelected) {
      this.selectedIds.clear();
      this.allSelected = false;
    } else {
      this.items.forEach(q => this.selectedIds.add(q.id));
      this.allSelected = true;
    }
  }

  updateAllSelected(): void {
    this.allSelected = this.items.length > 0 && this.selectedIds.size === this.items.length;
  }

  clearSelection(): void {
    this.selectedIds.clear();
    this.allSelected = false;
  }

  bulkPublish(): void {
    const ids = Array.from(this.selectedIds);
    ids.forEach(id => {
      const quiz = this.items.find(q => q.id === id);
      if (quiz && !quiz.isPublished) {
        this.service.publish(id, true).subscribe();
      }
    });
    setTimeout(() => {
      this.toast.show(`${ids.length} tests published`, 'success');
      this.clearSelection();
      this.load();
    }, 500);
  }

  bulkUnpublish(): void {
    const ids = Array.from(this.selectedIds);
    ids.forEach(id => {
      const quiz = this.items.find(q => q.id === id);
      if (quiz && quiz.isPublished) {
        this.service.publish(id, false).subscribe();
      }
    });
    setTimeout(() => {
      this.toast.show(`${ids.length} tests unpublished`, 'success');
      this.clearSelection();
      this.load();
    }, 500);
  }

  async bulkDelete(): Promise<void> {
    const count = this.selectedIds.size;
    const ok = await this.confirmDialog.open({
      title: `Delete ${count} test(s)?`,
      message: `This will permanently delete the selected tests.`,
      confirmText: 'Delete',
      cancelText: 'Cancel',
      tone: 'danger'
    });

    if (!ok) return;

    const ids = Array.from(this.selectedIds);
    ids.forEach(id => {
      this.service.delete(id).subscribe();
    });
    setTimeout(() => {
      this.toast.show(`${count} tests deleted`, 'success');
      this.clearSelection();
      this.load();
    }, 500);
  }

  bulkExport(): void {
    const ids = Array.from(this.selectedIds);
    this.service.bulkExport(ids).subscribe({
      next: () => {
        this.toast.show(`${ids.length} tests exported`, 'success');
      },
      error: () => this.toast.show('Export failed', 'error')
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

  onBulkActionSelected(event: Event): void {
    const select = event.target as HTMLSelectElement;
    const action = select.value;
    if (!action) return;

    switch (action) {
      case 'clear':
        this.clearSelection();
        break;
      case 'export':
        this.bulkExport();
        break;
      case 'publish':
        this.bulkPublish();
        break;
      case 'unpublish':
        this.bulkUnpublish();
        break;
      case 'delete':
        this.bulkDelete();
        break;
    }
    select.value = '';
  }

  onRowActionSelected(event: Event, quizId: number): void {
    const select = event.target as HTMLSelectElement;
    const action = select.value;
    if (!action) return;

    const quiz = this.items.find(q => q.id === quizId);
    if (!quiz) return;

    switch (action) {
      case 'export':
        this.exportQuiz(quizId);
        break;
      case 'edit':
        break;
      case 'publish':
        this.togglePublish(quiz);
        break;
      case 'delete':
        this.remove(quizId);
        break;
    }
    select.value = '';
  }
}
