import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { roleGuard } from './core/guards/role.guard';
import { LoginComponent } from './features/auth/login.component';
import { RegisterComponent } from './features/auth/register.component';
import { QuestionsListComponent } from './features/questions/questions-list.component';
import { QuestionFormComponent } from './features/questions/question-form.component';
import { QuizzesListComponent } from './features/quizzes/quizzes-list.component';
import { QuizFormComponent } from './features/quizzes/quiz-form.component';
import { QuizDetailsComponent } from './features/quizzes/quiz-details.component';
import { GameSessionsListComponent } from './features/game-sessions/game-sessions-list.component';
import { GameSessionControlComponent } from './features/game-sessions/game-session-control.component';
import { PlayerJoinComponent } from './features/player/player-join.component';
import { PlayerWaitingRoomComponent } from './features/player/player-waiting-room.component';
import { PlayerLiveQuestionComponent } from './features/player/player-live-question.component';
import { PlayerResultComponent } from './features/player/player-result.component';
import { PlayerLeaderboardComponent } from './features/player/player-leaderboard.component';
import { PlayerHistoryComponent } from './features/player/player-history.component';
import { TestModeListComponent } from './features/test-mode/test-mode-list.component';
import { TestModeAttemptComponent } from './features/test-mode/test-mode-attempt.component';
import { ResultsDashboardComponent } from './features/results/results-dashboard.component';

export const routes: Routes = [
  { path: 'auth/login', component: LoginComponent },
  { path: 'auth/register', component: RegisterComponent },

  {
    path: 'questions',
    canActivate: [authGuard, roleGuard],
    data: { roles: ['Admin', 'Host'] },
    children: [
      { path: '', component: QuestionsListComponent },
      { path: 'new', component: QuestionFormComponent },
      { path: ':id/edit', component: QuestionFormComponent }
    ]
  },

  {
    path: 'quizzes',
    canActivate: [authGuard, roleGuard],
    data: { roles: ['Admin', 'Host'] },
    children: [
      { path: '', component: QuizzesListComponent },
      { path: 'new', component: QuizFormComponent },
      { path: ':id/edit', component: QuizFormComponent },
      { path: ':id', component: QuizDetailsComponent }
    ]
  },

  {
    path: 'game-sessions',
    canActivate: [authGuard, roleGuard],
    data: { roles: ['Admin', 'Host'] },
    children: [
      { path: '', component: GameSessionsListComponent },
      { path: ':id/control', component: GameSessionControlComponent }
    ]
  },

  {
    path: 'results',
    canActivate: [authGuard, roleGuard],
    data: { roles: ['Admin', 'Host'] },
    component: ResultsDashboardComponent
  },

  { path: 'player/join', canActivate: [authGuard], component: PlayerJoinComponent },
  { path: 'player/join/:code', canActivate: [authGuard], component: PlayerJoinComponent },
  { path: 'player/session/:sessionId/waiting-room', canActivate: [authGuard], component: PlayerWaitingRoomComponent },
  { path: 'player/session/:sessionId/live', canActivate: [authGuard], component: PlayerLiveQuestionComponent },
  { path: 'player/session/:sessionId/leaderboard', canActivate: [authGuard], component: PlayerLeaderboardComponent },
  { path: 'player/session/:sessionId/result/:participantId', canActivate: [authGuard], component: PlayerResultComponent },
  {
    path: 'player/history',
    canActivate: [authGuard, roleGuard],
    data: { roles: ['Player', 'Admin', 'Host'] },
    component: PlayerHistoryComponent
  },

  { path: 'test-mode', canActivate: [authGuard], component: TestModeListComponent },
  { path: 'test-mode/attempt/:attemptId', canActivate: [authGuard], component: TestModeAttemptComponent },

  { path: '', redirectTo: '/auth/login', pathMatch: 'full' },
  { path: '**', redirectTo: '/auth/login' }
];
