import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { QuizService } from '../../core/services/quiz.service';
import { SafeRichTextPipe } from '../../shared/safe-rich-text.pipe';

@Component({
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule, SafeRichTextPipe],
  template: `
    <div class="details-shell">
      @if (quiz) {
        <section class="card hero-card">
          <div class="hero-head">
            <div>
              <p class="eyebrow">Test Builder</p>
              <h2>{{ quiz.title }}</h2>
              <p class="hero-copy">{{ quiz.description || 'No description provided yet.' }}</p>
            </div>

            <div class="hero-actions">
              <a [routerLink]="['/quizzes', quiz.id, 'edit']"><button type="button" class="secondary">Edit Test</button></a>
              <a routerLink="/quizzes"><button type="button" class="secondary">Back</button></a>
            </div>
          </div>

          <div class="hero-metrics">
            <div class="metric-card">
              <span>Status</span>
              <strong>{{ quiz.isPublished ? 'Published' : 'Draft' }}</strong>
            </div>
            <div class="metric-card">
              <span>Duration</span>
              <strong>{{ quiz.durationMinutes ? quiz.durationMinutes + ' min' : 'No limit' }}</strong>
            </div>
            <div class="metric-card">
              <span>Questions</span>
              <strong>{{ quiz.questions?.length || 0 }}</strong>
            </div>
            <div class="metric-card">
              <span>Marks</span>
              <strong>{{ marksLabel() }}</strong>
            </div>
          </div>

          <div class="tag-row">
            @for (category of quiz.categories || []; track category.id || category.name) {
              <span class="tag-chip">{{ category.name }}</span>
            }
          </div>
        </section>

        <section class="card builder-card">
          <div class="section-head">
            <div>
              <h3>Questions In This Test</h3>
              <p class="section-copy">Questions stay referenced, so the same question can appear in multiple tests without duplication.</p>
            </div>
            <a routerLink="/questions/new"><button type="button">Create New Question</button></a>
          </div>

          @if ((quiz.questions || []).length) {
            <div class="builder-actions">
              <button type="button" (click)="saveQuestionSettings()">Save Question Settings</button>
            </div>

            <div class="question-list">
              @for (item of quiz.questions; track item.id) {
                <article class="question-card">
                  <div class="question-card-head">
                    <div>
                      <p class="question-kicker">Question {{ item.order }}</p>
                      <h4>{{ item.question?.title || item.questionTitle }}</h4>
                    </div>
                    <button type="button" class="danger" (click)="removeQuestion(item.id)">Remove</button>
                  </div>

                  <div class="question-settings">
                    <div class="field">
                      <label>Order</label>
                      <input type="number" [(ngModel)]="item.order" min="1" />
                    </div>
                    <div class="field">
                      <label>Answer seconds</label>
                      <input type="number" [(ngModel)]="item.answerSeconds" min="5" max="300" />
                    </div>
                    <div class="field">
                      <label>Points override</label>
                      <input type="number" [(ngModel)]="item.pointsOverride" min="0" placeholder="Use base points if empty" />
                    </div>
                  </div>

                  @if (item.question?.imageUrl) {
                    <img class="question-image" [src]="item.question.imageUrl" alt="Question illustration" />
                  }

                  @if (item.question?.text) {
                    <div class="rich-text-content" [innerHTML]="item.question.text | safeRichText"></div>
                  }

                  @if ((item.question?.choices || []).length) {
                    <div class="choice-stack">
                      @for (choice of item.question?.choices || []; track choice.id || choice.order) {
                        <div class="choice-pill" [class.choice-pill-correct]="choice.isCorrect">
                          <strong>{{ choiceLabel(choice.order - 1) }}</strong>
                          <div class="choice-pill-content">
                            @if (choice.choiceText) {
                              <span>{{ choice.choiceText }}</span>
                            }
                            @if (choice.imageUrl) {
                              <img class="choice-pill-image" [src]="choice.imageUrl" [alt]="'Choice ' + choiceLabel(choice.order - 1)" />
                            }
                          </div>
                        </div>
                      }
                    </div>
                  }

                  @if (item.question?.explanation) {
                    <div class="explanation-box">
                      <span>Explanation</span>
                      <p>{{ item.question.explanation }}</p>
                    </div>
                  }
                </article>
              }
            </div>
          } @else {
            <div class="empty-state">
              <h4>No questions linked yet.</h4>
              <p>Use the reuse panel below or create a new question first.</p>
            </div>
          }
        </section>

        <section class="card source-card">
          <div class="section-head">
            <div>
              <h3>Reuse Questions From Other Tests</h3>
              <p class="section-copy">Browse an existing test, review its questions, and attach any of them here by reference.</p>
            </div>
          </div>

          <div class="source-filters">
            <div class="field">
              <label for="source-search">Search source tests</label>
              <input id="source-search" name="sourceSearch" [(ngModel)]="sourceSearch" placeholder="Search title or category" />
            </div>

            <div class="field">
              <label for="source-test">Select source test</label>
              <select id="source-test" name="sourceTest" [(ngModel)]="selectedSourceQuizId" (change)="loadSourceQuiz()">
                <option [ngValue]="0">Choose a test</option>
                @for (item of filteredSourceTests(); track item.id) {
                  <option [ngValue]="item.id">{{ item.title }} | {{ sourceCategoryLabel(item) }}</option>
                }
              </select>
            </div>
          </div>

          @if (sourceQuiz) {
            <div class="source-summary">
              <strong>{{ sourceQuiz.title }}</strong>
              <span>{{ sourceCategoryLabel(sourceQuiz) }}</span>
            </div>

            <div class="bulk-actions">
              <button type="button" class="secondary" (click)="selectAllSourceQuestions()">Select All</button>
              <button type="button" class="secondary" (click)="clearSourceSelection()">Clear</button>
              <button type="button" [disabled]="!selectedSourceQuestionIds.length" (click)="addSelectedSourceQuestions()">Add Selected Questions</button>
            </div>

            <div class="question-list">
              @for (item of sourceQuiz.questions || []; track item.id) {
                <article class="question-card" [class.question-card-disabled]="isAlreadyLinked(item.questionId)">
                  <div class="question-card-head">
                    <label class="checkbox-row">
                      <input
                        type="checkbox"
                        [checked]="isSourceQuestionSelected(item.questionId)"
                        [disabled]="isAlreadyLinked(item.questionId)"
                        (change)="toggleSourceQuestion(item.questionId, $event)" />
                      <span>{{ item.question?.title || item.questionTitle }}</span>
                    </label>
                    @if (isAlreadyLinked(item.questionId)) {
                      <span class="tag-chip">Already linked</span>
                    }
                  </div>

                  @if (item.question?.imageUrl) {
                    <img class="question-image" [src]="item.question.imageUrl" alt="Question illustration" />
                  }

                  @if (item.question?.text) {
                    <div class="rich-text-content" [innerHTML]="item.question.text | safeRichText"></div>
                  }

                  @if ((item.question?.choices || []).length) {
                    <div class="choice-stack">
                      @for (choice of item.question?.choices || []; track choice.id || choice.order) {
                        <div class="choice-pill" [class.choice-pill-correct]="choice.isCorrect">
                          <strong>{{ choiceLabel(choice.order - 1) }}</strong>
                          <div class="choice-pill-content">
                            @if (choice.choiceText) {
                              <span>{{ choice.choiceText }}</span>
                            }
                            @if (choice.imageUrl) {
                              <img class="choice-pill-image" [src]="choice.imageUrl" [alt]="'Choice ' + choiceLabel(choice.order - 1)" />
                            }
                          </div>
                        </div>
                      }
                    </div>
                  }
                </article>
              }
            </div>
          } @else if (loadingSourceQuiz) {
            <p>Loading source test...</p>
          } @else {
            <div class="empty-state">
              <h4>Select a source test</h4>
              <p>Its reusable questions will appear here.</p>
            </div>
          }
        </section>
      }

      @if (error) { <div class="alert">{{ error }}</div> }
    </div>
  `,
  styles: [`
    :host {
      display: block;
      min-width: 0;
    }

    .details-shell {
      display: grid;
      gap: 14px;
      min-width: 0;
    }

    .hero-card,
    .builder-card,
    .source-card {
      display: grid;
      gap: 14px;
    }

    .hero-head,
    .section-head,
    .question-card-head {
      display: flex;
      justify-content: space-between;
      gap: 12px;
      align-items: flex-start;
      flex-wrap: wrap;
    }

    .hero-head h2,
    .section-head h3 {
      margin: 0;
    }

    .hero-copy,
    .section-copy {
      margin: 6px 0 0;
      max-width: 64ch;
    }

    .hero-actions,
    .builder-actions,
    .bulk-actions {
      display: flex;
      gap: 8px;
      flex-wrap: wrap;
    }

    .hero-actions a {
      text-decoration: none;
    }

    .hero-metrics {
      display: grid;
      gap: 10px;
      grid-template-columns: repeat(4, minmax(0, 1fr));
    }

    .metric-card {
      min-width: 0;
      padding: 14px;
      border-radius: 18px;
      border: 1px solid var(--border);
      background: var(--surface-soft);
    }

    .metric-card span {
      display: block;
      margin-bottom: 6px;
      color: var(--muted);
      font-size: 0.76rem;
      font-weight: 700;
      letter-spacing: 0.08em;
      text-transform: uppercase;
    }

    .tag-row,
    .choice-stack {
      display: flex;
      flex-wrap: wrap;
      gap: 8px;
    }

    .tag-chip,
    .choice-pill {
      display: inline-flex;
      align-items: flex-start;
      gap: 8px;
      padding: 8px 12px;
      border-radius: 999px;
      border: 1px solid var(--border);
      background: var(--surface-soft);
    }

    .choice-pill-correct {
      border-color: var(--success-border);
      background: var(--success-tint);
    }

    .choice-pill-content {
      display: grid;
      gap: 8px;
    }

    .choice-pill-image {
      width: min(120px, 100%);
      border-radius: 14px;
      border: 1px solid var(--border);
      background: var(--surface);
    }

    .question-list {
      display: grid;
      gap: 12px;
    }

    .question-card {
      display: grid;
      gap: 12px;
      padding: 16px;
      border-radius: 20px;
      border: 1px solid var(--border);
      background: var(--surface-soft);
    }

    .question-card-disabled {
      opacity: 0.68;
    }

    .question-kicker {
      margin: 0 0 4px;
      color: var(--muted);
      font-size: 0.76rem;
      font-weight: 700;
      letter-spacing: 0.08em;
      text-transform: uppercase;
    }

    .question-card h4 {
      margin: 0;
    }

    .question-settings,
    .source-filters {
      display: grid;
      gap: 10px;
      grid-template-columns: repeat(3, minmax(0, 1fr));
    }

    .source-filters {
      grid-template-columns: repeat(2, minmax(0, 1fr));
    }

    .field {
      display: grid;
      gap: 6px;
      min-width: 0;
    }

    .question-image {
      width: min(320px, 100%);
      border-radius: 18px;
      border: 1px solid var(--border);
      background: var(--surface);
    }

    .checkbox-row {
      display: inline-flex;
      align-items: center;
      gap: 10px;
      font-weight: 700;
    }

    .source-summary {
      display: flex;
      justify-content: space-between;
      gap: 10px;
      align-items: center;
      flex-wrap: wrap;
      padding: 12px 14px;
      border-radius: 16px;
      border: 1px solid var(--border);
      background: var(--surface-soft);
    }

    .explanation-box {
      padding: 12px 14px;
      border-radius: 16px;
      border: 1px solid var(--border);
      background: var(--surface);
    }

    .explanation-box span {
      display: inline-flex;
      margin-bottom: 6px;
      color: var(--muted);
      font-size: 0.76rem;
      font-weight: 700;
      letter-spacing: 0.08em;
      text-transform: uppercase;
    }

    .explanation-box p {
      margin: 0;
      line-height: 1.55;
    }

    .empty-state {
      padding: 18px;
      border-radius: 18px;
      border: 1px dashed var(--border-strong);
      background: var(--surface-soft);
    }

    .empty-state h4 {
      margin: 0 0 6px;
    }

    .empty-state p {
      margin: 0;
    }

    @media (max-width: 900px) {
      .hero-metrics,
      .question-settings,
      .source-filters {
        grid-template-columns: 1fr;
      }
    }
  `]
})
export class QuizDetailsComponent implements OnInit {
  quiz: any;
  sourceTests: any[] = [];
  sourceQuiz: any;
  sourceSearch = '';
  selectedSourceQuizId = 0;
  selectedSourceQuestionIds: number[] = [];
  loadingSourceQuiz = false;
  error = '';
  private id = 0;

  constructor(
    private route: ActivatedRoute,
    private quizService: QuizService
  ) {}

  ngOnInit(): void {
    this.id = Number(this.route.snapshot.paramMap.get('id'));
    this.load();
    this.loadSourceTests();
  }

  load(): void {
    this.error = '';
    this.quizService.getById(this.id).subscribe({
      next: (res) => {
        this.quiz = res;
        this.quiz.questions = (this.quiz.questions || []).map((item: any, index: number) => ({
          ...item,
          order: Number(item?.order ?? index + 1),
          answerSeconds: Number(item?.answerSeconds ?? item?.question?.answerSeconds ?? 30),
          pointsOverride: item?.pointsOverride ?? null
        }));
      },
      error: (err) => this.error = err?.error?.message || 'Failed to load test'
    });
  }

  loadSourceTests(): void {
    this.quizService.getAll({ mode: 1, pageNumber: 1, pageSize: 200 }).subscribe({
      next: (res: any) => {
        this.sourceTests = (res.items || []).filter((item: any) => Number(item.id) !== this.id);
      },
      error: () => {}
    });
  }

  loadSourceQuiz(): void {
    if (!this.selectedSourceQuizId) {
      this.sourceQuiz = null;
      this.selectedSourceQuestionIds = [];
      return;
    }

    this.loadingSourceQuiz = true;
    this.selectedSourceQuestionIds = [];
    this.quizService.getById(this.selectedSourceQuizId).subscribe({
      next: (res) => {
        this.loadingSourceQuiz = false;
        this.sourceQuiz = res;
      },
      error: (err) => {
        this.loadingSourceQuiz = false;
        this.error = err?.error?.message || 'Failed to load source test';
      }
    });
  }

  filteredSourceTests(): any[] {
    const term = String(this.sourceSearch || '').trim().toLowerCase();
    if (!term) {
      return this.sourceTests;
    }

    return this.sourceTests.filter((item) => {
      const title = String(item?.title || '').toLowerCase();
      const categories = this.sourceCategoryLabel(item).toLowerCase();
      return title.includes(term) || categories.includes(term);
    });
  }

  toggleSourceQuestion(questionId: number, event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.checked) {
      if (!this.selectedSourceQuestionIds.includes(questionId)) {
        this.selectedSourceQuestionIds = [...this.selectedSourceQuestionIds, questionId];
      }
      return;
    }

    this.selectedSourceQuestionIds = this.selectedSourceQuestionIds.filter((item) => item !== questionId);
  }

  isSourceQuestionSelected(questionId: number): boolean {
    return this.selectedSourceQuestionIds.includes(questionId);
  }

  selectAllSourceQuestions(): void {
    const availableIds = (this.sourceQuiz?.questions || [])
      .filter((item: any) => !this.isAlreadyLinked(item.questionId))
      .map((item: any) => Number(item.questionId));

    this.selectedSourceQuestionIds = Array.from(new Set(availableIds));
  }

  clearSourceSelection(): void {
    this.selectedSourceQuestionIds = [];
  }

  addSelectedSourceQuestions(): void {
    if (!this.quiz || !this.selectedSourceQuestionIds.length) {
      return;
    }

    const nextOrderBase = this.nextQuestionOrder();
    const itemsToAdd = (this.sourceQuiz?.questions || [])
      .filter((item: any) => this.selectedSourceQuestionIds.includes(Number(item.questionId)))
      .filter((item: any) => !this.isAlreadyLinked(item.questionId))
      .map((item: any, index: number) => ({
        questionId: item.questionId,
        order: nextOrderBase + index,
        pointsOverride: item.pointsOverride ?? null,
        answerSeconds: item.answerSeconds ?? item.question?.answerSeconds ?? 30
      }));

    if (!itemsToAdd.length) {
      return;
    }

    this.quizService.addQuestions(this.id, itemsToAdd).subscribe({
      next: () => {
        this.selectedSourceQuestionIds = [];
        this.load();
        this.loadSourceQuiz();
      },
      error: (err) => this.error = err?.error?.message || 'Failed to add source questions'
    });
  }

  saveQuestionSettings(): void {
    const payload = (this.quiz?.questions || []).map((item: any, index: number) => ({
      questionId: Number(item.questionId),
      order: this.normalizePositive(item.order, index + 1),
      pointsOverride: this.normalizeOptionalNumber(item.pointsOverride),
      answerSeconds: this.normalizeBetween(item.answerSeconds, 5, 300, 30)
    }));

    this.quizService.addQuestions(this.id, payload).subscribe({
      next: () => this.load(),
      error: (err) => this.error = err?.error?.message || 'Failed to save question settings'
    });
  }

  removeQuestion(quizQuestionId: number): void {
    this.quizService.removeQuestion(this.id, quizQuestionId).subscribe({
      next: () => this.load(),
      error: (err) => this.error = err?.error?.message || 'Failed to remove question'
    });
  }

  isAlreadyLinked(questionId: number): boolean {
    return (this.quiz?.questions || []).some((item: any) => Number(item.questionId) === Number(questionId));
  }

  marksLabel(): string {
    const total = Number(this.quiz?.totalMarks ?? 0);
    const effective = Number(this.quiz?.effectiveTotalMarks ?? 0);
    return total > 0 ? `${total}` : `${effective || 0}`;
  }

  sourceCategoryLabel(item: any): string {
    const categories = Array.isArray(item?.categories) ? item.categories.map((category: any) => category?.name).filter(Boolean) : [];
    return categories.length ? categories.join(', ') : 'Uncategorized';
  }

  choiceLabel(index: number): string {
    return String.fromCharCode(65 + Math.max(0, index));
  }

  private nextQuestionOrder(): number {
    const currentOrders = (this.quiz?.questions || []).map((item: any) => Number(item.order) || 0);
    const maxOrder = currentOrders.length ? Math.max(...currentOrders) : 0;
    return maxOrder + 1;
  }

  private normalizePositive(value: any, fallback: number): number {
    const parsed = Number(value);
    if (!Number.isFinite(parsed) || parsed <= 0) return fallback;
    return Math.floor(parsed);
  }

  private normalizeBetween(value: any, min: number, max: number, fallback: number): number {
    const parsed = Number(value);
    if (!Number.isFinite(parsed)) return fallback;
    return Math.min(max, Math.max(min, Math.floor(parsed)));
  }

  private normalizeOptionalNumber(value: any): number | null {
    if (value === null || value === undefined || value === '') return null;
    const parsed = Number(value);
    if (!Number.isFinite(parsed) || parsed <= 0) return null;
    return Math.floor(parsed);
  }
}
