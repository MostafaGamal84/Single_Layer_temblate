import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { TestModeService } from '../../core/services/test-mode.service';

@Component({
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="card test-card">
      <h2>Test Mode</h2>
      <p class="sub">Select a published test quiz and start attempt.</p>

      @if (loading) {
        <p class="muted">Loading quizzes...</p>
      }

      @if (!loading && quizzes.length === 0 && !error) {
        <div class="alert">No published test quizzes found.</div>
      }

      @if (quizzes.length > 0) {
        <div class="table-wrap desktop-table">
          <table>
            <thead><tr><th>Quiz</th><th>Duration</th><th></th></tr></thead>
            <tbody>
              @for (q of quizzes; track q.id) {
                <tr>
                  <td>{{ q.title }}</td>
                  <td>{{ q.durationMinutes }} min</td>
                  <td><button (click)="start(q.id)">Start</button></td>
                </tr>
              }
            </tbody>
          </table>
        </div>

        <div class="mobile-list">
          @for (q of quizzes; track q.id) {
            <article class="mobile-item">
              <h3>{{ q.title }}</h3>
              <p>{{ q.durationMinutes }} min</p>
              <button (click)="start(q.id)">Start</button>
            </article>
          }
        </div>
      }

      @if (error) { <div class="alert">{{ error }}</div> }
    </div>
  `,
  styles: [`
    .test-card {
      max-width: 980px;
      margin: 0 auto;
      min-width: 0;
    }

    .sub {
      margin-bottom: 12px;
    }

    .muted {
      color: var(--muted);
    }

    .mobile-list {
      display: none;
      gap: 10px;
      min-width: 0;
      min-height: 300px;
    }

    .table-wrap {
      min-height: 400px;
    }

    @media (max-width: 760px) {
      .desktop-table {
        display: none;
      }

      .mobile-list {
        display: grid;
      }
    }
  `]
})
export class TestModeListComponent implements OnInit {
  quizzes: any[] = [];
  error = '';
  loading = false;

  constructor(private testService: TestModeService, private router: Router) {}

  ngOnInit(): void {
    this.loading = true;
    this.testService.quizzes({ pageNumber: 1, pageSize: 200 }).subscribe({
      next: (res: any) => {
        this.loading = false;
        this.quizzes = res.items || [];
      },
      error: (err) => {
        this.loading = false;
        this.error = err?.error?.message || 'Failed to load quizzes';
      }
    });
  }

  start(quizId: number): void {
    this.error = '';
    this.testService.start(quizId).subscribe({
      next: (res) => {
        const allowed = res?.allowed ?? res?.Allowed ?? true;
        if (allowed === false) {
          this.error = res?.message || 'You have reached the maximum number of attempts.';
          return;
        }
        const attemptId = res?.attemptId ?? res?.AttemptId ?? 0;
        if (attemptId > 0) {
          this.router.navigate(['/test-mode/attempt', attemptId]);
        }
      },
      error: (err) => this.error = err?.error?.message || 'Failed to start attempt'
    });
  }
}



