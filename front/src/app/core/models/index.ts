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
  imageUrl?: string;
  hasImage?: boolean;
  isCorrect: boolean;
  order: number;
}

export interface Category {
  id: number;
  name: string;
}

export interface Question {
  id: number;
  title: string;
  text: string;
  type: number;
  selectionMode: number;
  difficulty?: string;
  imageUrl?: string;
  explanation?: string;
  points: number;
  answerSeconds: number;
  choices: QuestionChoice[];
}

export interface QuizQuestion {
  id: number;
  questionId: number;
  questionTitle: string;
  order: number;
  pointsOverride?: number | null;
  answerSeconds: number;
  question?: Question;
}

export interface Quiz {
  id: number;
  title: string;
  description?: string;
  coverImageUrl?: string;
  mode: number;
  durationMinutes: number;
  totalMarks?: number | null;
  effectiveTotalMarks: number;
  isPublished: boolean;
  questionsCount: number;
  categories: Category[];
  questions?: QuizQuestion[];
}

export interface GameSession {
  id: number;
  quizId: number;
  quizTitle: string;
  quizCoverImageUrl?: string;
  joinCode: string;
  joinLink: string;
  status: number;
  accessType: number;
  questionFlowMode: number;
  scheduledStartAt?: string | null;
  scheduledEndAt?: string | null;
  durationMinutes?: number | null;
  currentQuestionIndex: number;
  participantsCount: number;
  categories: Category[];
}

export interface LeaderboardItem {
  participantId: number;
  displayName: string;
  totalScore: number;
  rank: number;
}
