export type UserRole = 'Admin' | 'Host' | 'Player';

export interface AuthUser {
  token: string;
  email?: string;
  role?: UserRole;
}

export interface PagedResult<T> {
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  items: T[];
}

export interface QuestionChoice {
  id?: number;
  choiceText: string;
  isCorrect: boolean;
  order: number;
}

export interface Question {
  id: number;
  title: string;
  text: string;
  type: number;
  difficulty?: string;
  points: number;
  answerSeconds: number;
  choices: QuestionChoice[];
}

export interface Quiz {
  id: number;
  title: string;
  description?: string;
  coverImageUrl?: string;
  mode: number;
  durationMinutes: number;
  isPublished: boolean;
  questionsCount: number;
}

export interface GameSession {
  id: number;
  quizId: number;
  quizTitle: string;
  quizCoverImageUrl?: string;
  joinCode: string;
  joinLink: string;
  status: number;
  questionFlowMode: number;
  currentQuestionIndex: number;
  participantsCount: number;
}

export interface LeaderboardItem {
  participantId: number;
  displayName: string;
  totalScore: number;
  rank: number;
}
