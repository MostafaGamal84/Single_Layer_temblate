import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { PagedResult, Quiz } from '../models';
import { map } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class QuizService {
  private base = `${environment.apiBaseUrl}/quizzes`;

  constructor(private http: HttpClient) {}

  getAll(params: any) {
    let query = new HttpParams();
    Object.entries(params || {}).forEach(([k, v]) => {
      if (v !== null && v !== undefined && v !== '') query = query.set(k, v as string);
    });

    return this.http.get<any>(this.base, { params: query }).pipe(
      map((res) => {
        const rootArray = this.toArray(res);
        const rawItems = rootArray.length
          ? rootArray
          : this.toArray(res?.items ?? res?.Items ?? res?.data ?? res?.Data);

        return {
          pageNumber: Number(res?.pageNumber ?? res?.PageNumber ?? params?.pageNumber ?? 1),
          pageSize: Number(res?.pageSize ?? res?.PageSize ?? params?.pageSize ?? 10),
          totalCount: Number(res?.totalCount ?? res?.TotalCount ?? rawItems.length),
          items: rawItems.map((x: any) => this.normalizeQuizItem(x))
        } as PagedResult<Quiz>;
      })
    );
  }

  getById(id: number) {
    return this.http.get<any>(`${this.base}/${id}`).pipe(map((res) => this.normalizeQuizDetails(res)));
  }
  create(payload: any) { return this.http.post<any>(this.base, payload); }
  update(id: number, payload: any) { return this.http.put<any>(`${this.base}/${id}`, payload); }
  uploadCoverImage(id: number, file: File) {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<any>(`${this.base}/${id}/cover-image`, formData);
  }
  delete(id: number) { return this.http.delete(`${this.base}/${id}`); }
  addQuestions(id: number, payload: any[]) { return this.http.post(`${this.base}/${id}/questions`, payload); }
  reorderQuestions(id: number, payload: any[]) { return this.http.put(`${this.base}/${id}/questions/reorder`, payload); }
  publish(id: number, isPublished: boolean) { return this.http.put(`${this.base}/${id}/publish`, { isPublished }); }

  private toArray(value: any): any[] {
    if (Array.isArray(value)) return value;
    if (value && Array.isArray(value.$values)) return value.$values;
    return [];
  }

  private normalizeQuizItem(item: any): Quiz {
    return {
      id: Number(item?.id ?? item?.Id ?? 0),
      title: String(item?.title ?? item?.Title ?? ''),
      description: item?.description ?? item?.Description ?? '',
      coverImageUrl: this.resolveAssetUrl(item?.coverImageUrl ?? item?.CoverImageUrl ?? ''),
      mode: Number(item?.mode ?? item?.Mode ?? 0),
      durationMinutes: Number(item?.durationMinutes ?? item?.DurationMinutes ?? 0),
      isPublished: Boolean(item?.isPublished ?? item?.IsPublished ?? false),
      questionsCount: Number(item?.questionsCount ?? item?.QuestionsCount ?? 0)
    };
  }

  private normalizeQuizDetails(item: any): any {
    const questions = this.toArray(item?.questions ?? item?.Questions).map((q: any) => ({
      id: Number(q?.id ?? q?.Id ?? 0),
      questionId: Number(q?.questionId ?? q?.QuestionId ?? 0),
      questionTitle: String(q?.questionTitle ?? q?.QuestionTitle ?? ''),
      order: Number(q?.order ?? q?.Order ?? 0),
      pointsOverride: q?.pointsOverride ?? q?.PointsOverride ?? null,
      answerSeconds: Number(q?.answerSeconds ?? q?.AnswerSeconds ?? 30)
    }));

    return {
      ...this.normalizeQuizItem(item),
      createdBy: item?.createdBy ?? item?.CreatedBy ?? null,
      createdAt: item?.createdAt ?? item?.CreatedAt ?? null,
      questions
    };
  }

  private resolveAssetUrl(value: any): string {
    const raw = String(value ?? '').trim();
    if (!raw) return '';
    if (/^https?:\/\//i.test(raw)) return raw;

    const apiRoot = environment.apiBaseUrl.replace(/\/api\/?$/i, '');
    return raw.startsWith('/') ? `${apiRoot}${raw}` : `${apiRoot}/${raw}`;
  }
}
