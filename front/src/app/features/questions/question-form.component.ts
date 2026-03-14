import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { EditorModule } from 'primeng/editor';
import { QuestionService } from '../../core/services/question.service';
import { SafeRichTextPipe } from '../../shared/safe-rich-text.pipe';

@Component({
  standalone: true,
  imports: [CommonModule, FormsModule, EditorModule, SafeRichTextPipe],
  template: `
    <div class="card question-form-card">
      <h2>{{ id ? 'Edit' : 'Create' }} Question</h2>

      <div class="col">
        <div class="col" style="gap:4px;">
          <label for="question-title">Title</label>
          <input id="question-title" name="questionTitle" [(ngModel)]="model.title" placeholder="Title" />
        </div>

        <div class="col editor-field">
          <label for="question-text-editor">Question text</label>
          <p-editor
            [(ngModel)]="model.text"
            name="questionText"
            styleClass="question-editor"
            [style]="{ height: '320px' }"
            [formats]="editorFormats"
            placeholder="Write the question exactly as students should see it">
            <ng-template pTemplate="header">
              <span class="ql-formats">
                <select class="ql-header" aria-label="Text style">
                  <option value="1">Heading</option>
                  <option value="2">Subheading</option>
                  <option selected></option>
                </select>
                <select class="ql-font" aria-label="Font family">
                  <option selected></option>
                  <option value="serif"></option>
                  <option value="monospace"></option>
                </select>
              </span>
              <span class="ql-formats">
                <button type="button" class="ql-bold" aria-label="Bold"></button>
                <button type="button" class="ql-italic" aria-label="Italic"></button>
                <button type="button" class="ql-underline" aria-label="Underline"></button>
              </span>
              <span class="ql-formats">
                <select class="ql-color" aria-label="Text color"></select>
                <select class="ql-background" aria-label="Highlight color"></select>
              </span>
              <span class="ql-formats">
                <button type="button" class="ql-list" value="ordered" aria-label="Ordered list"></button>
                <button type="button" class="ql-list" value="bullet" aria-label="Bullet list"></button>
                <select class="ql-align" aria-label="Text alignment">
                  <option selected></option>
                  <option value="center">Center</option>
                  <option value="right">Right</option>
                  <option value="justify">Justify</option>
                </select>
              </span>
              <span class="ql-formats">
                <button type="button" class="ql-link" aria-label="Insert link"></button>
                <button type="button" class="ql-image" aria-label="Insert image"></button>
                <button type="button" class="ql-code-block" aria-label="Code block"></button>
              </span>
              <span class="ql-formats">
                <button type="button" class="ql-clean" aria-label="Clear formatting"></button>
              </span>
            </ng-template>
          </p-editor>
          <small class="inline-note">Formatting like bold, underline, color, lists, alignment, links, and images will be saved with the question.</small>
        </div>

        @if (hasMeaningfulRichText(model.text)) {
          <div class="question-preview">
            <span class="preview-label">Student Preview</span>
            <div class="rich-text-content" [innerHTML]="model.text | safeRichText"></div>
          </div>
        }

        <div class="row">
          <div class="col" style="gap:4px;min-width:min(180px,100%);flex:1;">
            <label for="question-type">Question type</label>
            <select id="question-type" name="questionType" [(ngModel)]="model.type" (change)="onTypeChanged()">
              <option [ngValue]="1">Multiple Choice</option>
              <option [ngValue]="2">True / False</option>
              <option [ngValue]="3">Short Answer</option>
            </select>
          </div>

          <div class="col" style="gap:4px;min-width:min(120px,100%);">
            <label for="question-points">Points</label>
            <input id="question-points" name="questionPoints" type="number" [(ngModel)]="model.points" placeholder="Points" />
          </div>

          <div class="col" style="gap:4px;min-width:min(160px,100%);">
            <label for="question-answer-seconds">Answer time (sec)</label>
            <input id="question-answer-seconds" name="questionAnswerSeconds" type="number" [(ngModel)]="model.answerSeconds" min="5" max="300" placeholder="Answer time (sec)" />
          </div>

          <div class="col" style="gap:4px;min-width:min(180px,100%);flex:1;">
            <label for="question-difficulty">Difficulty</label>
            <input id="question-difficulty" name="questionDifficulty" [(ngModel)]="model.difficulty" placeholder="Difficulty (optional)" />
          </div>
        </div>
      </div>

      @if (model.type !== 3) {
        <h3>Choices</h3>
        @for (choice of model.choices; track $index) {
          <div class="row" style="margin-bottom:8px;">
            <div class="col" style="gap:4px;flex:1;">
              <label [attr.for]="'question-choice-' + $index">Choice {{ $index + 1 }}</label>
              <input [attr.id]="'question-choice-' + $index" [attr.name]="'questionChoice' + $index" [(ngModel)]="choice.choiceText" [placeholder]="'Choice ' + ($index + 1)" />
            </div>
            <label [attr.for]="'question-choice-correct-' + $index">
              <input [attr.id]="'question-choice-correct-' + $index" [attr.name]="'questionChoiceCorrect' + $index" type="checkbox" [(ngModel)]="choice.isCorrect" /> Correct
            </label>
            <button type="button" class="danger" (click)="removeChoice($index)">X</button>
          </div>
        }
        <button type="button" class="secondary" (click)="addChoice()">Add choice</button>
      }

      @if (model.type === 3) {
        <h3>Short Answer Key</h3>
        <div class="col" style="gap:4px;">
          <label for="question-short-answer-key">Expected answer</label>
          <input id="question-short-answer-key" name="questionShortAnswerKey" [(ngModel)]="shortAnswerKey" placeholder="Expected answer" />
        </div>
      }

      <div class="row" style="margin-top:12px;">
        <button type="button" [disabled]="loading" (click)="save()">{{ loading ? 'Saving...' : 'Save' }}</button>
        <button type="button" class="secondary" (click)="goBack()">Back</button>
      </div>

      @if (error) { <div class="alert" style="margin-top:10px;">{{ error }}</div> }
    </div>
  `,
  styles: [`
    .question-form-card {
      display: grid;
      gap: 14px;
    }

    .question-form-card h2,
    .question-form-card h3 {
      margin-bottom: 0;
    }

    .editor-field {
      gap: 8px;
    }

    .question-preview {
      padding: 14px 16px;
      border-radius: 18px;
      border: 1px solid var(--border);
      background: var(--surface-soft);
    }

    .preview-label {
      display: inline-flex;
      margin-bottom: 10px;
      color: var(--muted-strong);
      font-size: 0.74rem;
      font-weight: 800;
      letter-spacing: 0.1em;
      text-transform: uppercase;
    }
  `]
})
export class QuestionFormComponent implements OnInit {
  id?: number;
  loading = false;
  error = '';
  shortAnswerKey = '';
  readonly editorFormats = ['header', 'font', 'bold', 'italic', 'underline', 'color', 'background', 'list', 'align', 'link', 'image', 'code-block'];

  model: any = {
    title: '',
    text: '',
    type: 1,
    difficulty: '',
    points: 100,
    answerSeconds: 30,
    choices: [
      { choiceText: '', isCorrect: false, order: 1 },
      { choiceText: '', isCorrect: true, order: 2 }
    ]
  };

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private service: QuestionService
  ) {}

  ngOnInit(): void {
    const rawId = this.route.snapshot.paramMap.get('id');
    this.id = rawId ? Number(rawId) : undefined;
    if (this.id) {
      this.service.getById(this.id).subscribe((res: any) => {
        this.model = {
          ...res,
          text: String(res?.text ?? ''),
          answerSeconds: Number(res?.answerSeconds ?? 30),
          choices: res.choices || []
        };
        if (this.model.type === 3) {
          this.shortAnswerKey = this.model.choices?.find((x: any) => x.isCorrect)?.choiceText || '';
        }
      });
    }
  }

  onTypeChanged(): void {
    if (this.model.type === 2) {
      this.model.choices = [
        { choiceText: 'True', isCorrect: true, order: 1 },
        { choiceText: 'False', isCorrect: false, order: 2 }
      ];
    }

    if (this.model.type === 3) {
      this.model.choices = [];
      this.shortAnswerKey = '';
    }

    if (this.model.type === 1 && this.model.choices.length < 2) {
      this.model.choices = [
        { choiceText: '', isCorrect: false, order: 1 },
        { choiceText: '', isCorrect: true, order: 2 }
      ];
    }
  }

  addChoice(): void {
    this.model.choices.push({ choiceText: '', isCorrect: false, order: this.model.choices.length + 1 });
  }

  removeChoice(index: number): void {
    this.model.choices.splice(index, 1);
    this.model.choices.forEach((x: any, i: number) => x.order = i + 1);
  }

  save(): void {
    this.loading = true;
    this.error = '';
    this.model.answerSeconds = this.normalizeAnswerSeconds(this.model.answerSeconds);
    this.model.text = this.normalizeRichText(this.model.text);

    if (!this.hasMeaningfulRichText(this.model.text)) {
      this.loading = false;
      this.error = 'Question text is required';
      return;
    }

    if (this.model.type === 3) {
      const normalizedKey = (this.shortAnswerKey || '').trim();
      if (!normalizedKey) {
        this.loading = false;
        this.error = 'Short answer key is required';
        return;
      }

      this.model.choices = [{ choiceText: normalizedKey, isCorrect: true, order: 1 }];
    } else {
      this.model.choices.forEach((x: any, i: number) => x.order = i + 1);
    }

    const request$ = this.id
      ? this.service.update(this.id, this.model)
      : this.service.create(this.model);

    request$.subscribe({
      next: () => {
        this.loading = false;
        this.router.navigate(['/questions']);
      },
      error: (err) => {
        this.loading = false;
        this.error = err?.error?.message || 'Save failed';
      }
    });
  }

  goBack(): void {
    this.router.navigate(['/questions']);
  }

  hasMeaningfulRichText(value: string | null | undefined): boolean {
    return this.extractPlainText(value).length > 0;
  }

  private normalizeAnswerSeconds(value: any): number {
    const parsed = Number(value);
    if (!Number.isFinite(parsed)) return 30;
    if (parsed < 5) return 5;
    if (parsed > 300) return 300;
    return Math.floor(parsed);
  }

  private normalizeRichText(value: string | null | undefined): string {
    return String(value ?? '').trim();
  }

  private extractPlainText(value: string | null | undefined): string {
    return String(value ?? '')
      .replace(/<[^>]*>/g, ' ')
      .replace(/&nbsp;/gi, ' ')
      .replace(/&#160;/gi, ' ')
      .replace(/\s+/g, ' ')
      .trim();
  }
}


