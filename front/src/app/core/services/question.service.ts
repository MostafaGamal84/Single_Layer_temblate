import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { PagedResult, Question } from '../models';
import { map } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class QuestionService {
  private base = `${environment.apiBaseUrl}/questions`;

  constructor(private http: HttpClient) {}

  getAll(params: { pageNumber?: number; pageSize?: number; search?: string; type?: number | null; difficulty?: string | null }) {
    let query = new HttpParams();
    Object.entries(params).forEach(([k, v]) => {
      if (v !== null && v !== undefined && v !== '') query = query.set(k, v as string);
    });

    return this.http.get<any>(this.base, { params: query }).pipe(
      map((res) => {
        const rootArray = this.toArray(res);
        const rawItems = rootArray.length
          ? rootArray
          : this.toArray(res?.items ?? res?.Items ?? res?.data ?? res?.Data);
        const pageNumber = Number(res?.pageNumber ?? res?.PageNumber ?? params.pageNumber ?? 1);
        const pageSize = Number(res?.pageSize ?? res?.PageSize ?? params.pageSize ?? 10);
        const totalCount = Number(res?.totalCount ?? res?.TotalCount ?? rawItems.length);

        return {
          pageNumber,
          pageSize,
          totalCount,
          items: rawItems.map((x: any) => this.normalizeQuestion(x))
        } as PagedResult<Question>;
      })
    );
  }

  getById(id: number) {
    return this.http.get<any>(`${this.base}/${id}`).pipe(map((res) => this.normalizeQuestion(res)));
  }

  create(payload: any) {
    return this.http.post<any>(this.base, payload).pipe(map((res) => this.normalizeQuestion(res)));
  }

  update(id: number, payload: any) {
    return this.http.put<any>(`${this.base}/${id}`, payload).pipe(map((res) => this.normalizeQuestion(res)));
  }

  delete(id: number) {
    return this.http.delete(`${this.base}/${id}`);
  }

  private toArray(value: any): any[] {
    if (Array.isArray(value)) return value;
    if (value && Array.isArray(value.$values)) return value.$values;
    return [];
  }

  private normalizeQuestion(item: any): Question {
    const rawChoices = this.toArray(item?.choices ?? item?.Choices);

    return {
      id: Number(item?.id ?? item?.Id ?? 0),
      title: String(item?.title ?? item?.Title ?? item?.questionTitle ?? item?.QuestionTitle ?? ''),
      text: String(item?.text ?? item?.Text ?? ''),
      type: Number(item?.type ?? item?.Type ?? 0),
      selectionMode: Number(item?.selectionMode ?? item?.SelectionMode ?? 1),
      difficulty: item?.difficulty ?? item?.Difficulty ?? undefined,
      imageUrl: this.resolveAssetUrl(item?.imageUrl ?? item?.ImageUrl ?? ''),
      explanation: item?.explanation ?? item?.Explanation ?? undefined,
      points: Number(item?.points ?? item?.Points ?? item?.pointsOverride ?? item?.PointsOverride ?? 0),
      answerSeconds: Number(item?.answerSeconds ?? item?.AnswerSeconds ?? 30),
      choices: rawChoices.map((c: any, index: number) => ({
        id: Number(c?.id ?? c?.Id ?? 0),
        choiceText: String(c?.choiceText ?? c?.ChoiceText ?? ''),
        imageUrl: this.resolveAssetUrl(c?.imageUrl ?? c?.ImageUrl ?? ''),
        hasImage: Boolean(c?.hasImage ?? c?.HasImage ?? false),
        isCorrect: Boolean(c?.isCorrect ?? c?.IsCorrect ?? false),
        order: Number(c?.order ?? c?.Order ?? index + 1)
      }))
    };
  }

  uploadImage(id: number, file: File) {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<any>(`${this.base}/${id}/image`, formData);
  }

  uploadChoiceImage(questionId: number, choiceId: number, file: File) {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<any>(`${this.base}/${questionId}/choices/${choiceId}/image`, formData);
  }

  private resolveAssetUrl(value: any): string {
    const raw = String(value ?? '').trim();
    if (!raw) return '';
    if (/^https?:\/\//i.test(raw)) return raw;

    const apiRoot = environment.apiBaseUrl.replace(/\/api\/?$/i, '');
    return raw.startsWith('/') ? `${apiRoot}${raw}` : `${apiRoot}/${raw}`;
  }
}
