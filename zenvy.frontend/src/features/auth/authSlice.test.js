import authSlice, { logout, setAuth, setError, setLoading } from './authSlice';

const authReducer = authSlice.reducer;

const signedOutState = {
  token: null,
  user: null,
  isAuthenticated: false,
  loading: false,
  error: null,
};

describe('authSlice', () => {
  it('stores a successful login and clears transient state', () => {
    const state = authReducer({ ...signedOutState, loading: true, error: 'Previous error' }, setAuth({
      token: 'access-token',
      user: { userId: '1', fullName: 'Asha', role: 'Admin' },
    }));

    expect(state).toEqual({
      token: 'access-token',
      user: { userId: '1', fullName: 'Asha', role: 'Admin' },
      isAuthenticated: true,
      loading: false,
      error: null,
    });
  });

  it('tracks loading and request errors independently', () => {
    const loadingState = authReducer(signedOutState, setLoading(true));
    const errorState = authReducer(loadingState, setError('Invalid credentials'));

    expect(errorState.loading).toBe(true);
    expect(errorState.error).toBe('Invalid credentials');
  });

  it('removes in-memory authentication details on logout', () => {
    const state = authReducer({
      token: 'access-token', user: { role: 'Admin' }, isAuthenticated: true, loading: true, error: 'error',
    }, logout());

    expect(state).toEqual(signedOutState);
  });
});
