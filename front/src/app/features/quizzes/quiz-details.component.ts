import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { QuizService } from '../../core/services/quiz.service';
import { QuestionService } from '../../core/services/question.service';

@Component({
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  template: `
    <div class="card details-shell">
      @if (quiz) {
        <div class="quiz-head">
          <div>
            <h2>{{ quiz.title }}</h2>
            <p>{{ quiz.description }}</p>
          </div>
          <a routerLink="/quizzes"><button type="button" class="secondary">Back</button></a>
        </div>

        <h3>Add Questions</h3>
        <div class="add-bar">
          <select [(ngModel)]="selectedQuestionId" [disabled]="loadingBank || bank.length === 0">
            <option [ngValue]="0">Select question</option>
            @for (q of bank; track q.id) {
              <option [ngValue]="q.id">{{ displayQuestionTitle(q) }}</option>
            }
          </select>
          <input type="number" [(ngModel)]="nextOrder" placeholder="Order" />
          <button type="button" [disabled]="loadingBank || bank.length === 0" (click)="addQuestion()">Add</button>
        </div>
        @if (loadingBank) { <p>Loading question bank...</p> }
        @if (!loadingBank && bank.length === 0) { <p>No questions available in Question Bank.</p> }

        <h3 class="section-title">Quiz Questions</h3>
        <div class="table-wrap desktop-table">
          <table>
            <thead>
              <tr><th>Order</th><th>Answer Time (sec)</th><th>Question</th><th>Actions</th></tr>
            </thead>
            <tbody>
              @for (q of quiz.questions; track q.id) {
                <tr>
                  <td><input type="number" [(ngModel)]="q.order" class="order-input" /></td>
                  <td>{{ q.answerSeconds }}</td>
                  <td>{{ displayQuestionTitle(q) }}</td>
                  <td><button type="button" class="secondary" (click)="saveOrder()">Save</button></td>
                </tr>
              }
            </tbody>
          </table>
        </div>

        <div class="mobile-list">
          @for (q of quiz.questions; track q.id) {
            <article class="mobile-item">
              <strong>{{ displayQuestionTitle(q) }}</strong>

              <div class="mobile-metrics">
                <div class="mobile-metric">
                  <span>Order</span>
                  <input type="number" [(ngModel)]="q.order" class="order-input" />
                </div>
                <div class="mobile-metric">
                  <span>Answer Time</span>
                  <strong>{{ q.answerSeconds }} sec</strong>
                </div>
              </div>

              <button type="button" class="secondary" (click)="saveOrder()">Save</button>
            </article>
          }
        </div>
      }

      @if (error) { <div class="alert details-alert">{{ error }}</div> }
    </div>
  `,
  styles: [`
    :host {
      display: block;
      min-width: 0;
      max-width: 100%;
    }

    .details-shell {
      display: grid;
      gap: 14px;
      min-width: 0;
    }

    .quiz-head {
      display: flex;
      justify-content: space-between;
      gap: 10px;
      align-items: flex-start;
      flex-wrap: wrap;
    }

    .quiz-head h2,
    .section-title {
      margin: 0;
    }

    .quiz-head a {
      text-decoration: none;
    }

    .add-bar {
      display: grid;
      grid-template-columns: minmax(0, 1fr) 110px auto;
      gap: 10px;
      min-width: 0;
      align-items: end;
    }

    .order-input {
      width: 100%;
      min-width: 0;
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

    .mobile-item strong {
      color: var(--text);
      line-height: 1.4;
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
      margin-bottom: 6px;
      color: var(--muted);
      font-size: 0.72rem;
      font-weight: 700;
      letter-spacing: 0.08em;
      text-transform: uppercase;
    }

    .details-alert {
      margin-top: 4px;
    }

    @media (max-width: 760px) {
      .add-bar {
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

      .mobile-metrics {
        grid-template-columns: 1fr;
      }
    }
  `]
})
export class QuizDetailsComponent implements OnInit {
  quiz: any;
  bank: any[] = [];
  selectedQuestionId = 0;
  nextOrder = 1;
  loadingBank = false;
  error = '';
  private id = 0;

  constructor(
    private route: ActivatedRoute,
    private quizService: QuizService,
    private questionService: QuestionService
  ) {}

  ngOnInit(): void {
    this.id = Number(this.route.snapshot.paramMap.get('id'));
    this.load();
    this.loadQuestionBank();
  }

  load(): void {
    this.quizService.getById(this.id).subscribe({
      next: (res) => {
        this.quiz = res;
        this.quiz.questions = (this.quiz.questions || []).map((x: any) => ({
          ...x,
          answerSeconds: Number(x?.answerSeconds ?? 30)
        }));
        this.nextOrder = (res.questions?.length || 0) + 1;
      },
      error: (err) => this.error = err?.error?.message || 'Failed to load quiz'
    });
  }

  addQuestion(): void {
    this.error = '';

    const questionId = Number(this.selectedQuestionId);
    if (!questionId) {
      this.error = 'Please select a question first.';
      return;
    }

    const alreadyAdded = (this.quiz?.questions || []).some((x: any) => Number(x.questionId) === questionId);
    if (alreadyAdded) {
      this.error = 'This question is already added to this quiz.';
      return;
    }

    this.quizService.addQuestions(this.id, [{ questionId: this.selectedQuestionId, order: this.nextOrder }]).subscribe({
      next: () => {
        this.selectedQuestionId = 0;
        this.load();
      },
      error: (err) => this.error = err?.error?.message || 'Failed to add question'
    });
  }

  saveOrder(): void {
    const payload = this.quiz.questions.map((x: any) => ({ quizQuestionId: x.id, order: x.order }));
    this.quizService.reorderQuestions(this.id, payload).subscribe({
      next: () => this.load(),
      error: (err) => this.error = err?.error?.message || 'Failed to reorder'
    });
  }

  private loadQuestionBank(): void {
    this.loadingBank = true;
    this.questionService.getAll({ pageNumber: 1, pageSize: 200 }).subscribe({
      next: (res: any) => {
        this.bank = (res.items || []).filter((x: any) => x && Number(x.id) > 0);
        this.loadingBank = false;
      },
      error: (err) => {
        this.loadingBank = false;
        this.error = err?.error?.message || 'Failed to load question bank';
      }
    });
  }

  displayQuestionTitle(item: any): string {
    return item?.title ?? item?.questionTitle ?? item?.text ?? `#${item?.id ?? ''}`;
  }
}
