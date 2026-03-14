import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { PlayerService } from '../../core/services/player.service';
import { isPlatformBrowser } from '@angular/common';
import { Inject, PLATFORM_ID } from '@angular/core';

@Component({
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="center-stage">
      <div class="card spotlight-card narrow-card">
        <p class="eyebrow">Live access</p>
        <h2>Join Live Session</h2>
        <p class="page-intro">Enter the session code and the name that should appear on the live leaderboard.</p>

        <div class="col">
          <div class="field">
            <label for="player-join-code">Join code</label>
            <input id="player-join-code" name="playerJoinCode" [(ngModel)]="joinCode" placeholder="Join code" />
          </div>

          <div class="field">
            <label for="player-display-name">Display name</label>
            <input id="player-display-name" name="playerDisplayName" [(ngModel)]="displayName" placeholder="Display name" />
          </div>

          <button [disabled]="loading" (click)="join()">{{ loading ? 'Joining...' : 'Join session' }}</button>
        </div>

        @if (error) { <div class="alert" style="margin-top:12px;">{{ error }}</div> }
      </div>
    </div>
  `
})
export class PlayerJoinComponent implements OnInit {
  joinCode = '';
  displayName = '';
  loading = false;
  error = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private service: PlayerService,
    @Inject(PLATFORM_ID) private platformId: object
  ) {}

  ngOnInit(): void {
    const code = this.route.snapshot.paramMap.get('code');
    if (code) this.joinCode = code;
  }

  join(): void {
    this.loading = true;
    this.error = '';

    this.service.join({ joinCode: this.joinCode, displayName: this.displayName }).subscribe({
      next: (res) => {
        this.loading = false;
        if (isPlatformBrowser(this.platformId)) {
          localStorage.setItem('participant_id', String(res.participantId));
          localStorage.setItem('participant_token', res.participantToken);
          localStorage.setItem('participant_display_name', res.displayName || this.displayName);
          localStorage.setItem('participant_join_status', String(res.joinStatus ?? 1));
        }
        this.router.navigate(['/player/session', res.sessionId, 'waiting-room']);
      },
      error: (err) => {
        this.loading = false;
        this.error = err?.error?.message || 'Join failed';
      }
    });
  }
}
