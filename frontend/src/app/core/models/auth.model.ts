/** POST /users/login body. The backend authenticates by email (not username). */
export interface LoginRequest {
  email: string;
  password: string;
}

/** Backend LoginCommandResult. */
export interface LoginResult {
  accessToken: string;
  refreshToken: string;
}
