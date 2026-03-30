import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { QuestionService } from '../../core/services/question.service';
import { QuizService } from '../../core/services/quiz.service';
import { QuestionCategoryService } from '../../core/services/question-category.service';
import { ToastService } from '../../core/services/toast.service';
import { PagedResult, Question } from '../../core/models';

@Component({
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <div class="question-bank-page">
      <div class="page-header">
        <div>
          <p class="eyebrow">Question Bank</p>
          <h2>Questions Library</h2>
          <p class="header-desc">Browse, create, and assign questions to tests</p>
        </div>
        <div class="header-actions">
          <button type="button" class="secondary" (click)="showAssignModal = true" [disabled]="selectedQuestions.size === 0">
            Assign to Test ({{ selectedQuestions.size }})
          </button>
          <a routerLink="/questions/new"><button type="button">Create Question</button></a>
        </div>
      </div>

      <div class="filters-bar">
        <div class="field search-field">
          <input
            type="text"
            [(ngModel)]="searchTerm"
            placeholder="Search questions..."
            (keyup.enter)="loadQuestions()" />
        </div>
        <div class="field">
          <select [(ngModel)]="typeFilter" (change)="loadQuestions()">
            <option [ngValue]="0">All Types</option>
            <option [ngValue]="1">Multiple Choice</option>
            <option [ngValue]="2">True/False</option>
            <option [ngValue]="3">Short Answer</option>
          </select>
        </div>
        <div class="field">
          <select [(ngModel)]="modeFilter" (change)="loadQuestions()">
            <option [ngValue]="0">All Modes</option>
            <option [ngValue]="1">Single Answer</option>
            <option [ngValue]="2">Multiple Answers</option>
          </select>
        </div>
        <div class="field">
          <select [(ngModel)]="categoryFilter" (change)="loadQuestions()">
            <option [ngValue]="0">All Categories</option>
            @for (cat of categories; track cat.id) {
              <option [ngValue]="cat.id">{{ cat.name }}</option>
            }
          </select>
        </div>
      </div>

      @if (loading) {
        <div class="loading-state">
          <p>Loading questions...</p>
        </div>
      }

      @if (!loading && questions.length === 0) {
        <div class="empty-state">
          <h4>No questions found</h4>
          <p>Create your first question to get started</p>
          <a routerLink="/questions/new"><button type="button">Create Question</button></a>
        </div>
      }

      @if (!loading && questions.length > 0) {
        <div class="questions-list">
          @for (q of questions; track q.id) {
            <article class="question-row" [class.selected]="selectedQuestions.has(q.id)" (click)="toggleQuestion(q.id)">
              <div class="row-select">
                <input type="checkbox" [checked]="selectedQuestions.has(q.id)" (click)="$event.stopPropagation()" (change)="toggleQuestion(q.id)" />
              </div>
              
              <div class="row-content">
                <div class="row-main">
                  <div class="row-title">{{ q.title }}</div>
                  <div class="row-text">{{ getPreviewText(q.text) }}</div>
                  
                  @if (q.choices && q.choices.length > 0) {
                    <div class="row-choices">
                      @for (choice of q.choices; track choice.id || choice.order) {
                        <span class="choice-chip" [class.correct]="choice.isCorrect">
                          @if (choice.imageUrl) {
                            <img [src]="choice.imageUrl" alt="" class="choice-icon" />
                          }
                          {{ choice.choiceText || 'Image' }}
                          @if (choice.isCorrect) {
                            <span class="check-icon">✓</span>
                          }
                        </span>
                      }
                    </div>
                  }
                </div>
                
                <div class="row-meta">
                  <div class="meta-item" title="Question type">
                    <span class="meta-label">Type:</span>
                    <span class="meta-badge type-badge">{{ getTypeName(q.type) }}</span>
                  </div>
                  <div class="meta-item" title="Points for correct answer">
                    <span class="meta-label">Points:</span>
                    <span class="meta-badge">{{ q.points }}</span>
                  </div>
                  <div class="meta-item" title="Time to answer in seconds">
                    <span class="meta-label">Time:</span>
                    <span class="meta-badge">{{ q.answerSeconds }}s</span>
                  </div>
                  @if (q.difficulty) {
                    <div class="meta-item" title="Difficulty level">
                      <span class="meta-label">Difficulty:</span>
                      <span class="meta-badge difficulty-{{ q.difficulty.toLowerCase() }}">{{ q.difficulty }}</span>
                    </div>
                  }
                  @if (q.categoryName) {
                    <div class="meta-item" title="Question category">
                      <span class="meta-label">Category:</span>
                      <span class="meta-badge category-badge">{{ q.categoryName }}</span>
                    </div>
                  }
                  @if (q.quizTitle) {
                    <div class="meta-item" title="Assigned to this test">
                      <span class="meta-label">Test:</span>
                      <span class="meta-badge linked-badge">{{ q.quizTitle }}</span>
                    </div>
                  }
                  @if (q.quizId && !q.quizTitle) {
                    <div class="meta-item" title="Question is linked to a test">
                      <span class="meta-label">Status:</span>
                      <span class="meta-badge linked-badge">Linked</span>
                    </div>
                  }
                  @if (q.isOwnedByQuiz) {
                    <div class="meta-item" title="Question belongs to a test and cannot be moved">
                      <span class="meta-label">Ownership:</span>
                      <span class="meta-badge owned-badge">Test Owned</span>
                    </div>
                  }
                </div>
              </div>
              
              <div class="row-actions" (click)="$event.stopPropagation()">
                <a [routerLink]="['/questions', q.id, 'edit']"><button type="button" class="secondary btn-sm">Edit</button></a>
                <button type="button" class="secondary btn-sm" (click)="assignQuestion(q)">Assign</button>
                <button type="button" class="secondary btn-sm btn-danger" (click)="deleteQuestion(q)">Delete</button>
              </div>
            </article>
          }
        </div>

        @if (totalPages > 1) {
          <div class="pagination">
            <button type="button" class="secondary" [disabled]="page === 1" (click)="goToPage(page - 1)">Previous</button>
            <span class="page-info">Page {{ page }} of {{ totalPages }}</span>
            <button type="button" class="secondary" [disabled]="page === totalPages" (click)="goToPage(page + 1)">Next</button>
          </div>
        }
      }

      @if (error) { <div class="alert">{{ error }}</div> }
    </div>

    @if (showAssignModal) {
      <div class="modal-overlay" (click)="closeAssignModal()">
        <div class="modal-content" (click)="$event.stopPropagation()">
          <div class="modal-header">
            <h3>Assign Questions to Test</h3>
            <button type="button" class="close-btn" (click)="closeAssignModal()">✕</button>
          </div>
          <div class="modal-body">
            <div class="selected-info">
              <span class="count-badge">{{ selectedQuestions.size }}</span> question(s) selected
            </div>

            <div class="field">
              <label for="assign-quiz">Select Test</label>
              <select id="assign-quiz" [(ngModel)]="selectedQuizId">
                <option [ngValue]="0">Choose a test...</option>
                @for (quiz of availableQuizzes; track quiz.id) {
                  <option [ngValue]="quiz.id">{{ quiz.title }}</option>
                }
              </select>
            </div>
          </div>
          <div class="modal-footer">
            <button type="button" class="secondary" (click)="closeAssignModal()">Cancel</button>
            <button type="button" [disabled]="selectedQuestions.size === 0 || selectedQuizId === 0" (click)="assignSelectedToQuiz()">
              Assign
            </button>
          </div>
        </div>
      </div>
    }
  `,
  styles: [`
    .question-bank-page {
      display: grid;
      gap: 20px;
    }

    .page-header {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
      flex-wrap: wrap;
      gap: 16px;
    }

    .page-header h2 { margin: 0; }
    .header-desc { margin: 4px 0 0; color: var(--muted); }
    .header-actions { display: flex; gap: 8px; align-items: center; }
    .header-actions a { text-decoration: none; }

    .filters-bar {
      display: flex;
      gap: 10px;
      flex-wrap: wrap;
      padding: 14px;
      background: var(--surface);
      border: 1px solid var(--border);
      border-radius: 12px;
    }

    .search-field { flex: 1; min-width: 180px; }
    .field { display: grid; gap: 6px; }
    .field select, .field input {
      padding: 10px 12px;
      border: 1px solid var(--border);
      border-radius: 10px;
      background: var(--input-bg);
      min-height: 42px;
      font-size: 0.9rem;
    }

    .loading-state {
      text-align: center;
      padding: 48px 20px;
      color: var(--muted);
    }

    .empty-state {
      text-align: center;
      padding: 48px 20px;
      border: 2px dashed var(--border);
      border-radius: 16px;
      background: var(--surface-soft);
    }

    .questions-list {
      display: flex;
      flex-direction: column;
      gap: 12px;
    }

    .question-row {
      display: flex;
      gap: 14px;
      align-items: flex-start;
      padding: 14px 16px;
      background: var(--surface);
      border: 1px solid var(--border);
      border-radius: 12px;
      cursor: pointer;
      transition: all 0.15s ease;
    }

    .question-row:hover {
      border-color: var(--border-strong);
      box-shadow: var(--shadow-card);
    }

    .question-row.selected {
      border-color: var(--primary);
      background: var(--primary-tint);
    }

    .row-select {
      padding-top: 2px;
    }

    .row-select input[type="checkbox"] {
      width: 18px;
      height: 18px;
      cursor: pointer;
      accent-color: var(--primary);
    }

    .row-content {
      flex: 1;
      display: flex;
      flex-direction: column;
      gap: 10px;
      min-width: 0;
    }

    .row-main {
      display: flex;
      flex-direction: column;
      gap: 6px;
    }

    .row-title {
      font-weight: 700;
      font-size: 1.05rem;
      color: var(--text);
    }

    .row-text {
      color: var(--muted);
      font-size: 0.9rem;
      line-height: 1.4;
    }

    .row-choices {
      display: flex;
      flex-wrap: wrap;
      gap: 8px;
      margin-top: 8px;
    }

    .choice-chip {
      display: inline-flex;
      align-items: center;
      gap: 4px;
      padding: 5px 10px;
      border-radius: 8px;
      font-size: 0.8rem;
      background: var(--surface-soft);
      border: 1px solid var(--border);
      color: var(--muted);
    }

    .choice-chip.correct {
      background: var(--success-tint);
      border-color: var(--success-border);
      color: var(--success);
    }

    .choice-icon {
      width: 16px;
      height: 16px;
      border-radius: 4px;
      object-fit: cover;
    }

    .check-icon {
      font-weight: 700;
      margin-left: 2px;
    }

    .row-meta {
      display: flex;
      flex-wrap: wrap;
      gap: 6px;
    }

    .meta-item {
      display: flex;
      align-items: center;
      gap: 4px;
    }

    .meta-label {
      font-size: 0.72rem;
      color: var(--muted);
      font-weight: 600;
    }

    .meta-badge {
      display: inline-block;
      padding: 3px 8px;
      border-radius: 5px;
      font-size: 0.72rem;
      font-weight: 600;
      background: var(--surface-soft);
      border: 1px solid var(--border);
      color: var(--text-muted);
    }

    .type-badge {
      background: var(--primary-tint);
      border-color: var(--primary-border);
      color: var(--primary);
    }

    .linked-badge {
      background: var(--info-tint);
      border-color: var(--info-border);
      color: var(--info);
    }

    .owned-badge {
      background: var(--warning-tint);
      border-color: var(--warning-border);
      color: var(--warning);
    }

    .difficulty-easy {
      background: var(--success-tint);
      border-color: var(--success-border);
      color: var(--success);
    }

    .difficulty-medium {
      background: var(--warning-tint);
      border-color: var(--warning-border);
      color: var(--warning);
    }

    .difficulty-hard {
      background: var(--danger-tint);
      border-color: var(--danger-border);
      color: var(--danger);
    }

    .category-badge {
      background: var(--primary-tint);
      border-color: var(--primary-border);
      color: var(--primary);
    }

    .row-actions {
      display: flex;
      gap: 6px;
      flex-shrink: 0;
    }

    .btn-sm {
      padding: 6px 10px;
      font-size: 0.78rem;
      min-height: 32px;
    }

    .btn-danger:hover {
      border-color: var(--danger-border);
      color: var(--danger);
    }

    .pagination {
      display: flex;
      justify-content: center;
      align-items: center;
      gap: 16px;
      padding: 20px;
    }

    .page-info {
      color: var(--muted);
      font-size: 0.9rem;
    }

    .modal-overlay {
      position: fixed;
      top: 0;
      left: 0;
      right: 0;
      bottom: 0;
      background: var(--overlay-bg);
      display: flex;
      align-items: center;
      justify-content: center;
      z-index: 1000;
      padding: 16px;
      backdrop-filter: blur(4px);
    }

    .modal-content {
      background: var(--dialog-panel-bg);
      border: 1px solid var(--dialog-panel-border);
      border-radius: 18px;
      padding: 24px;
      width: 100%;
      max-width: 440px;
      box-shadow: var(--dialog-panel-shadow);
    }

    .modal-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 16px;
    }

    .modal-header h3 { margin: 0; font-size: 1.1rem; }
    .close-btn {
      background: none;
      border: none;
      font-size: 1.4rem;
      cursor: pointer;
      color: var(--muted);
      padding: 4px 8px;
      border-radius: 8px;
    }
    .close-btn:hover {
      background: var(--surface-soft);
      color: var(--text);
    }

    .modal-body { display: grid; gap: 14px; }
    .modal-footer { display: flex; justify-content: flex-end; gap: 8px; margin-top: 20px; }

    .selected-info {
      display: flex;
      align-items: center;
      gap: 10px;
      padding: 10px 14px;
      background: var(--primary-tint);
      border: 1px solid var(--primary-border);
      border-radius: 10px;
      font-weight: 600;
      color: var(--primary);
    }

    .count-badge {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      width: 26px;
      height: 26px;
      border-radius: 50%;
      background: var(--primary);
      color: white;
      font-size: 0.85rem;
    }

    @media (max-width: 900px) {
      .question-row {
        flex-direction: column;
        gap: 12px;
      }
      .row-actions {
        width: 100%;
        justify-content: flex-start;
      }
    }

    @media (max-width: 600px) {
      .page-header {
        flex-direction: column;
        align-items: stretch;
      }
      .header-actions {
        flex-direction: column;
      }
      .filters-bar {
        flex-direction: column;
      }
      .row-actions {
        flex-wrap: wrap;
      }
    }
  `]
})
export class QuestionBankComponent implements OnInit {
  questions: any[] = [];
  categories: any[] = [];
  loading = false;
  error = '';
  searchTerm = '';
  typeFilter = 0;
  modeFilter = 0;
  categoryFilter = 0;
  page = 1;
  pageSize = 12;
  totalPages = 1;

  selectedQuestions = new Set<number>();
  showAssignModal = false;
  selectedQuizId = 0;
  availableQuizzes: any[] = [];

  constructor(
    private questionService: QuestionService,
    private quizService: QuizService,
    private categoryService: QuestionCategoryService,
    private router: Router,
    private toast: ToastService
  ) {}

  ngOnInit(): void {
    this.loadQuestions();
    this.loadAvailableQuizzes();
    this.loadCategories();
  }

  loadCategories(): void {
    this.categoryService.getAll().subscribe({
      next: (cats) => this.categories = cats,
      error: () => {}
    });
  }

  loadQuestions(): void {
    this.loading = true;
    this.error = '';

    const params: any = {
      pageNumber: this.page,
      pageSize: this.pageSize
    };
    if (this.searchTerm) params.search = this.searchTerm;
    if (this.typeFilter) params.type = this.typeFilter;
    if (this.modeFilter) params.selectionMode = this.modeFilter;
    if (this.categoryFilter) params.categoryId = this.categoryFilter;

    this.questionService.getAll(params).subscribe({
      next: (res: PagedResult<Question>) => {
        this.loading = false;
        this.questions = res.items;
        this.totalPages = Math.ceil(res.totalCount / this.pageSize);
      },
      error: (err) => {
        this.loading = false;
        this.error = err?.error?.message || 'Failed to load questions';
      }
    });
  }

  loadAvailableQuizzes(): void {
    this.quizService.getAll({ mode: 1, pageNumber: 1, pageSize: 200 }).subscribe({
      next: (res: any) => {
        this.availableQuizzes = res.items || [];
      }
    });
  }

  toggleQuestion(id: number): void {
    if (this.selectedQuestions.has(id)) {
      this.selectedQuestions.delete(id);
    } else {
      this.selectedQuestions.add(id);
    }
  }

  assignQuestion(question: Question): void {
    this.selectedQuestions.clear();
    this.selectedQuestions.add(question.id);
    this.showAssignModal = true;
  }

  assignSelectedToQuiz(): void {
    if (this.selectedQuestions.size === 0 || this.selectedQuizId === 0) return;

    const questionIds = Array.from(this.selectedQuestions);
    const items = questionIds.map((qId, index) => ({
      questionId: qId,
      order: index + 1,
      pointsOverride: null,
      answerSeconds: null
    }));

    this.quizService.addQuestions(this.selectedQuizId, items).subscribe({
      next: () => {
        this.closeAssignModal();
        this.loadQuestions();
        this.toast.success('Questions assigned successfully');
      },
      error: (err) => {
        this.error = err?.error?.message || 'Failed to assign questions';
      }
    });
  }

  closeAssignModal(): void {
    this.showAssignModal = false;
    this.selectedQuizId = 0;
    this.selectedQuestions.clear();
  }

  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages) return;
    this.page = page;
    this.loadQuestions();
  }

  getTypeName(type: number): string {
    const types: Record<number, string> = { 1: 'Multiple Choice', 2: 'True/False', 3: 'Short Answer' };
    return types[type] || 'Unknown';
  }

  getPreviewText(text: string): string {
    const plain = text.replace(/<[^>]*>/g, '').replace(/&nbsp;/g, ' ').trim();
    return plain.length > 150 ? plain.substring(0, 150) + '...' : plain;
  }

  deleteQuestion(question: any): void {
    if (confirm(`Delete question "${question.title}"?`)) {
      this.questionService.delete(question.id).subscribe({
        next: () => {
          this.questions = this.questions.filter(q => q.id !== question.id);
          this.selectedQuestions.delete(question.id);
          this.toast.success('Question deleted');
        },
        error: (err) => {
          this.toast.error(err?.error?.message || 'Failed to delete question');
        }
      });
    }
  }
}
