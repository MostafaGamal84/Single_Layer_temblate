import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { QuizService } from '../../core/services/quiz.service';
import { of, switchMap } from 'rxjs';

@Component({
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="card test-form-card">
      <div class="form-head">
        <div>
          <p class="eyebrow">Test Builder</p>
          <h2>{{ id ? 'Edit Test' : 'Create Test' }}</h2>
          <p class="intro-copy">Define the test shell first. Questions, reuse rules, and sessions are managed after this step.</p>
        </div>
        <button type="button" class="secondary" (click)="cancel()">Back</button>
      </div>

      <div class="form-grid">
        <div class="field span-2">
          <label for="test-title">Title</label>
          <input id="test-title" name="testTitle" [(ngModel)]="model.title" placeholder="Unit 1 Midterm" />
        </div>

        <div class="field span-2">
          <label for="test-description">Description</label>
          <textarea id="test-description" name="testDescription" rows="4" [(ngModel)]="model.description" placeholder="What this test covers, instructions, or reuse notes"></textarea>
        </div>

        <div class="field">
          <label for="test-duration">Duration (minutes)</label>
          <input id="test-duration" name="testDurationMinutes" type="number" min="0" [(ngModel)]="model.durationMinutes" placeholder="60" />
        </div>

        <div class="field">
          <label for="test-total-marks">Total marks (optional)</label>
          <input id="test-total-marks" name="testTotalMarks" type="number" min="0" [(ngModel)]="model.totalMarks" placeholder="Auto-calculate if empty" />
        </div>

        <div class="field">
          <label for="test-status">Status</label>
          <select id="test-status" name="testStatus" [(ngModel)]="model.isPublished">
            <option [ngValue]="false">Draft</option>
            <option [ngValue]="true">Published</option>
          </select>
        </div>

        <div class="field">
          <label for="test-cover-image">Cover image</label>
          <input id="test-cover-image" name="testCoverImage" type="file" accept="image/png,image/jpeg,image/webp" (change)="onCoverFileSelected($event)" />
          <small class="inline-note">Allowed: JPG, PNG, WEBP up to 5 MB.</small>
        </div>

        <div class="field span-2">
          <label for="test-category-input">Categories</label>
          <div class="category-entry">
            <input
              id="test-category-input"
              name="testCategoryInput"
              [(ngModel)]="categoryInput"
              placeholder="Type a category and press Enter"
              (keydown.enter)="addCategoryFromInput($event)"
              (keydown)="handleCategorySeparators($event)" />
            <button type="button" class="secondary" (click)="addCategoryFromInput()">Add</button>
          </div>
          <small class="inline-note">Each test should belong to one or more categories for filtering and session search.</small>
        </div>
      </div>

      @if (model.categories.length) {
        <div class="category-chip-grid">
          @for (category of model.categories; track category) {
            <button type="button" class="chip chip-selected" (click)="removeCategory(category)">
              {{ category }}
              <span>Remove</span>
            </button>
          }
        </div>
      }

      @if (suggestedCategoryList.length) {
        <div class="suggestion-block">
          <span class="suggestion-label">Existing categories</span>
          <div class="category-chip-grid">
            @for (category of suggestedCategoryList; track category) {
              <button type="button" class="chip" (click)="applySuggestedCategory(category)">{{ category }}</button>
            }
          </div>
        </div>
      }

      @if (coverPreviewUrl) {
        <div class="cover-preview">
          <span class="suggestion-label">Cover preview</span>
          <img [src]="coverPreviewUrl" alt="Test cover preview" class="preview-image" />
        </div>
      }

      <div class="action-row">
        <button type="button" [disabled]="loading" (click)="save()">{{ loading ? 'Saving...' : 'Save Test' }}</button>
        <button type="button" class="secondary" [disabled]="loading" (click)="cancel()">Cancel</button>
      </div>

      @if (error) { <div class="alert">{{ error }}</div> }
    </div>
  `,
  styles: [`
    .test-form-card {
      display: grid;
      gap: 16px;
    }

    .form-head {
      display: flex;
      justify-content: space-between;
      gap: 12px;
      align-items: flex-start;
      flex-wrap: wrap;
    }

    .form-head h2 {
      margin: 0;
    }

    .intro-copy {
      margin: 6px 0 0;
      max-width: 58ch;
    }

    .form-grid {
      display: grid;
      gap: 12px;
      grid-template-columns: repeat(2, minmax(0, 1fr));
    }

    .span-2 {
      grid-column: span 2;
    }

    .field {
      display: grid;
      gap: 6px;
      min-width: 0;
    }

    .category-entry {
      display: grid;
      gap: 10px;
      grid-template-columns: minmax(0, 1fr) auto;
    }

    .category-chip-grid {
      display: flex;
      flex-wrap: wrap;
      gap: 8px;
    }

    .chip {
      display: inline-flex;
      align-items: center;
      gap: 8px;
      min-height: 38px;
      padding: 8px 12px;
      border-radius: 999px;
      border: 1px solid var(--border);
      background: var(--surface-soft);
      color: var(--text);
      box-shadow: none;
    }

    .chip span {
      color: var(--muted);
      font-size: 0.78rem;
    }

    .chip-selected {
      border-color: var(--input-focus-border);
      background: var(--ring);
    }

    .suggestion-block,
    .cover-preview {
      display: grid;
      gap: 8px;
    }

    .suggestion-label {
      color: var(--muted);
      font-size: 0.78rem;
      font-weight: 700;
      letter-spacing: 0.08em;
      text-transform: uppercase;
    }

    .preview-image {
      width: min(220px, 100%);
      border-radius: 18px;
      border: 1px solid var(--border);
      background: var(--surface-soft);
      object-fit: cover;
    }

    .action-row {
      display: flex;
      gap: 10px;
      flex-wrap: wrap;
    }

    @media (max-width: 760px) {
      .form-grid {
        grid-template-columns: 1fr;
      }

      .span-2 {
        grid-column: auto;
      }

      .category-entry {
        grid-template-columns: 1fr;
      }
    }
  `]
})
export class QuizFormComponent implements OnInit {
  id?: number;
  loading = false;
  error = '';
  categoryInput = '';
  coverPreviewUrl = '';
  existingCategories: string[] = [];
  private selectedCoverFile: File | null = null;

  model: any = {
    title: '',
    description: '',
    durationMinutes: 60,
    totalMarks: null,
    isPublished: false,
    mode: 1,
    categories: [] as string[]
  };

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private service: QuizService
  ) {}

  ngOnInit(): void {
    this.loadCategorySuggestions();

    const rawId = this.route.snapshot.paramMap.get('id');
    this.id = rawId ? Number(rawId) : undefined;
    if (!this.id) {
      return;
    }

    this.service.getById(this.id).subscribe((res: any) => {
      this.model = {
        title: res.title,
        description: res.description,
        durationMinutes: res.durationMinutes,
        totalMarks: res.totalMarks ?? null,
        isPublished: !!res.isPublished,
        mode: 1,
        categories: (res.categories || []).map((item: any) => String(item?.name ?? '')).filter(Boolean)
      };
      this.coverPreviewUrl = res.coverImageUrl || '';
    });
  }

  save(): void {
    this.error = '';

    if (!this.model.title?.trim()) {
      this.error = 'Test title is required.';
      return;
    }

    if (!this.model.categories.length) {
      this.error = 'At least one category is required.';
      return;
    }

    this.loading = true;
    const payload = {
      ...this.model,
      title: String(this.model.title || '').trim(),
      description: String(this.model.description || '').trim(),
      totalMarks: this.normalizeOptionalNumber(this.model.totalMarks),
      durationMinutes: this.normalizeNumber(this.model.durationMinutes, 0),
      categories: this.model.categories
    };

    const request$ = this.id ? this.service.update(this.id, payload) : this.service.create(payload);
    request$.pipe(
      switchMap((quiz: any) => {
        const quizId = Number(quiz?.id ?? quiz?.Id ?? this.id ?? 0);
        if (!this.selectedCoverFile || !quizId) {
          return of(quiz);
        }

        return this.service.uploadCoverImage(quizId, this.selectedCoverFile).pipe(
          switchMap(() => of(quiz))
        );
      })
    ).subscribe({
      next: (quiz: any) => {
        this.loading = false;
        const quizId = Number(quiz?.id ?? quiz?.Id ?? this.id ?? 0);
        this.router.navigate(quizId ? ['/quizzes', quizId] : ['/quizzes']);
      },
      error: (err) => {
        this.loading = false;
        this.error = err?.error?.message || 'Save failed';
      }
    });
  }

  cancel(): void {
    this.router.navigate(['/quizzes']);
  }

  addCategoryFromInput(event?: Event): void {
    event?.preventDefault();
    const value = String(this.categoryInput || '').trim();
    if (!value) {
      return;
    }

    if (!this.model.categories.some((item: string) => item.toLowerCase() === value.toLowerCase())) {
      this.model.categories = [...this.model.categories, value];
    }

    this.categoryInput = '';
  }

  handleCategorySeparators(event: KeyboardEvent): void {
    if (event.key !== ',') {
      return;
    }

    event.preventDefault();
    this.addCategoryFromInput();
  }

  removeCategory(category: string): void {
    this.model.categories = this.model.categories.filter((item: string) => item !== category);
  }

  applySuggestedCategory(category: string): void {
    if (this.model.categories.some((item: string) => item.toLowerCase() === category.toLowerCase())) {
      return;
    }

    this.model.categories = [...this.model.categories, category];
  }

  get suggestedCategoryList(): string[] {
    return this.existingCategories.filter((category) =>
      !this.model.categories.some((item: string) => item.toLowerCase() === category.toLowerCase())
    );
  }

  onCoverFileSelected(event: Event): void {
    this.error = '';
    const input = event.target as HTMLInputElement;
    const file = input.files && input.files.length ? input.files[0] : null;

    if (!file) {
      this.selectedCoverFile = null;
      return;
    }

    const allowedTypes = ['image/jpeg', 'image/png', 'image/webp'];
    if (!allowedTypes.includes(file.type)) {
      this.selectedCoverFile = null;
      this.error = 'Only JPG, PNG, or WEBP images are allowed.';
      input.value = '';
      return;
    }

    const maxSize = 5 * 1024 * 1024;
    if (file.size > maxSize) {
      this.selectedCoverFile = null;
      this.error = 'Image size must be 5 MB or less.';
      input.value = '';
      return;
    }

    this.selectedCoverFile = file;
    this.coverPreviewUrl = URL.createObjectURL(file);
  }

  private loadCategorySuggestions(): void {
    this.service.getCategories().subscribe({
      next: (items) => {
        this.existingCategories = items.map((item) => item.name).filter(Boolean);
      },
      error: () => {}
    });
  }

  private normalizeNumber(value: any, fallback: number): number {
    const parsed = Number(value);
    if (!Number.isFinite(parsed)) return fallback;
    return Math.max(fallback, Math.floor(parsed));
  }

  private normalizeOptionalNumber(value: any): number | null {
    if (value === null || value === undefined || value === '') return null;
    const parsed = Number(value);
    if (!Number.isFinite(parsed) || parsed <= 0) return null;
    return Math.floor(parsed);
  }
}
