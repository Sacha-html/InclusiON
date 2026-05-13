export interface UpdateLoginMethodResponse {
  updated: boolean;
  loginMethodId: number;
  loginMethodName: string;
  temporaryPassword?: string;
}

export interface SupervisorCandidate {
  userId: string;
  fullName: string;
  type: 'Professional' | 'Family';
  relationship?: string;
  avatarColor?: string;
}
