import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { QuizService } from '../../core/services/quiz.service';
import { QuizQuestionsComponent } from './quiz-questions.component';
import { QuizAccessComponent } from './quiz-access.component';

@Component({
  standalone: true,
  imports: [CommonModule, RouterLink, QuizQuestionsComponent, QuizAccessComponent],
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

          <div class="tab-nav">
            <button type="button" [class.active]="activeTab === 'questions'" (click)="activeTab = 'questions'">Questions</button>
            <button type="button" [class.active]="activeTab === 'access'" (click)="activeTab = 'access'">Access Settings</button>
          </div>
        </section>

        @if (activeTab === 'questions') {
          <app-quiz-questions
            [quizId]="quiz.id"
            [quizTitle]="quiz.title"
            [questions]="quiz.questions || []"
            (questionsChanged)="load()">
          </app-quiz-questions>
        }

        @if (activeTab === 'access') {
          <section class="card access-card">
            <app-quiz-access [quizId]="quiz.id" (accessChanged)="load()"></app-quiz-access>
          </section>
        }
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
    .access-card {
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

    .tab-nav {
      display: flex;
      gap: 4px;
      border-bottom: 1px solid var(--border);
      padding-bottom: 0;
    }

    .tab-nav button {
      padding: 10px 20px;
      background: none;
      border: none;
      border-bottom: 2px solid transparent;
      cursor: pointer;
      font-weight: 600;
      color: var(--muted);
      transition: all 0.2s;
    }

    .tab-nav button:hover {
      color: var(--text);
    }

    .tab-nav button.active {
      color: var(--text);
      border-bottom-color: var(--input-focus-border);
    }

    @media (max-width: 900px) {
      .hero-metrics,
      .question-settings,
      .source-filters {
        grid-template-columns: 1fr;
      }

      .tab-nav {
        overflow-x: auto;
      }
    }
  `]
})
export class QuizDetailsComponent implements OnInit {
  quiz: any;
  activeTab = 'questions';
  error = '';
  private id = 0;

  constructor(
    private route: ActivatedRoute,
    private quizService: QuizService
  ) {}

  ngOnInit(): void {
    this.id = Number(this.route.snapshot.paramMap.get('id'));
    this.load();
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

  marksLabel(): string {
    const total = Number(this.quiz?.totalMarks ?? 0);
    const effective = Number(this.quiz?.effectiveTotalMarks ?? 0);
    return total > 0 ? `${total}` : `${effective || 0}`;
  }
}
