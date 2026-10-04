import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import api from '../lib/api';
import PaymentsPage from './PaymentsPage';

jest.mock('../lib/api', () => ({
  __esModule: true,
  default: { get: jest.fn(), put: jest.fn() },
}));

jest.mock('../signalr/signalrProvider', () => ({
  useSignalR: () => ({ lastPaymentChange: null }),
}));

describe('PaymentsPage', () => {
  beforeEach(() => {
    api.get.mockResolvedValue({ data: [{
      paymentId: 21,
      orderId: 14,
      paymentMethodId: 2,
      methodName: 'UPI',
      amount: 850,
      transactionRef: null,
      status: 'PENDING',
      paymentDate: '2026-10-04T10:00:00',
    }] });
    api.put.mockResolvedValue({});
  });

  it('submits a payment status and transaction reference for reconciliation', async () => {
    const user = userEvent.setup();
    render(<PaymentsPage />);

    const status = await screen.findByLabelText('Status for payment 21');
    await user.selectOptions(status, 'COMPLETED');
    await user.type(screen.getByLabelText('Transaction reference for payment 21'), 'UPI-REF-123');
    await user.click(screen.getByRole('button', { name: 'Save status' }));

    await waitFor(() => expect(api.put).toHaveBeenCalledWith('/api/v1/payments/21/status', {
      status: 'COMPLETED',
      transactionRef: 'UPI-REF-123',
    }));
    expect(await screen.findByText(/Payment 21 updated/)).toBeInTheDocument();
  });
});
