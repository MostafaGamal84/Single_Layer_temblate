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
    <div class="card">
      <h2>{{ id ? 'Edit' : 'Create' }} Quiz</h2>
      <div class="col">
        <div class="col" style="gap:4px;">
          <label for="quiz-title">Title</label>
          <input id="quiz-title" name="quizTitle" [(ngModel)]="model.title" placeholder="Title" />
        </div>

        <div class="col" style="gap:4px;">
          <label for="quiz-description">Description</label>
          <textarea id="quiz-description" name="quizDescription" rows="3" [(ngModel)]="model.description" placeholder="Description"></textarea>
        </div>

        <div class="col" style="gap:4px;">
          <label for="quiz-cover-image">Cover image</label>
          <input id="quiz-cover-image" name="quizCoverImage" type="file" accept="image/png,image/jpeg,image/webp" (change)="onCoverFileSelected($event)" />
          <small class="inline-note">Allowed: JPG, PNG, WEBP (max 5 MB)</small>
        </div>

        @if (coverPreviewUrl) {
          <div class="col" style="gap:6px;">
            <label>Preview</label>
            <img
              [src]="coverPreviewUrl"
              alt="Quiz cover preview"
              class="image-preview" />
          </div>
        }

        <div class="row">
          <div class="col" style="gap:4px;min-width:min(200px,100%);flex:1;">
            <label for="quiz-mode">Mode</label>
            <select id="quiz-mode" name="quizMode" [(ngModel)]="model.mode">
              <option [ngValue]="1">Test</option>
              <option [ngValue]="2">Game</option>
            </select>
          </div>
          <div class="col" style="gap:4px;min-width:min(200px,100%);flex:1;">
            <label for="quiz-duration">Duration (minutes)</label>
            <input id="quiz-duration" name="quizDurationMinutes" type="number" [(ngModel)]="model.durationMinutes" placeholder="Duration (minutes)" />
          </div>
        </div>
      </div>

      <div class="row" style="margin-top:12px;">
        <button [disabled]="loading" (click)="save()">{{ loading ? 'Saving...' : 'Save' }}</button>
        <button class="secondary" (click)="cancel()">Cancel</button>
      </div>

      @if (error) { <div class="alert" style="margin-top:10px;">{{ error }}</div> }
    </div>
  `
})
export class QuizFormComponent implements OnInit {
  id?: number;
  loading = false;
  error = '';
  coverPreviewUrl = '';
  private selectedCoverFile: File | null = null;

  model: any = {
    title: '',
    description: '',
    coverImageUrl: '',
    mode: 1,
    durationMinutes: 15
  };

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private service: QuizService
  ) {}

  ngOnInit(): void {
    const rawId = this.route.snapshot.paramMap.get('id');
    this.id = rawId ? Number(rawId) : undefined;
    if (this.id) {
      this.service.getById(this.id).subscribe((res) => {
        this.model = {
          title: res.title,
          description: res.description,
          coverImageUrl: res.coverImageUrl || '',
          mode: res.mode,
          durationMinutes: res.durationMinutes
        };
        this.coverPreviewUrl = this.model.coverImageUrl || '';
      });
    }
  }

  save(): void {
    this.error = '';
    this.loading = true;
    const request$ = this.id ? this.service.update(this.id, this.model) : this.service.create(this.model);
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
      next: () => {
        this.loading = false;
        this.router.navigate(['/quizzes']);
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
}


