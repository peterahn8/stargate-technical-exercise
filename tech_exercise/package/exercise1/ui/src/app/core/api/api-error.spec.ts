import { HttpClient, HttpRequest, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { firstValueFrom, throwError } from 'rxjs';
import { ApiError, apiErrorInterceptor } from './api-error';

describe('[UI-3] apiErrorInterceptor', () => {
  let client: HttpClient;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([apiErrorInterceptor])),
        provideHttpClientTesting(),
      ],
    });
    client = TestBed.inject(HttpClient);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  async function failure(respond: (url: string) => void): Promise<ApiError> {
    const call = firstValueFrom(client.get('/api/Person/Nobody'));
    respond('/api/Person/Nobody');
    try {
      await call;
    } catch (error) {
      return error as ApiError;
    }
    throw new Error('Expected the call to fail');
  }

  it("uses the API's own message for an error response", async () => {
    const error = await failure((url) =>
      http
        .expectOne(url)
        .flush(
          { success: false, message: "Person 'Nobody' not found.", responseCode: 404 },
          { status: 404, statusText: 'Not Found' },
        ),
    );

    expect(error).toBeInstanceOf(ApiError);
    expect(error.status).toBe(404);
    expect(error.message).toBe("Person 'Nobody' not found.");
  });

  it("explains when the API can't be reached", async () => {
    const error = await failure((url) => http.expectOne(url).error(new ProgressEvent('error')));

    expect(error.status).toBe(0);
    expect(error.message).toBe("The API couldn't be reached. Is it running?");
  });

  it.each([502, 503, 504])(
    'explains a gateway error with no API message (%i) as the API being unreachable',
    async (status) => {
      // The dev server's proxy answers this way when the API isn't running.
      const error = await failure((url) =>
        http.expectOne(url).flush('', { status, statusText: 'Gateway error' }),
      );

      expect(error.status).toBe(status);
      expect(error.message).toBe("The API couldn't be reached. Is it running?");
    },
  );

  it('gives a generic message for a failure that is not an HTTP error', async () => {
    const call = firstValueFrom(
      apiErrorInterceptor(new HttpRequest('GET', '/api/Person'), () =>
        throwError(() => new Error('socket hang up')),
      ),
    );

    await expect(call).rejects.toBeInstanceOf(ApiError);
    await expect(call).rejects.toMatchObject({ message: 'An unexpected error occurred.' });
  });

  it('falls back to the status code when the response has no message', async () => {
    const error = await failure((url) =>
      http.expectOne(url).flush('Internal Server Error', { status: 500, statusText: 'Error' }),
    );

    expect(error.status).toBe(500);
    expect(error.message).toBe('The request failed (HTTP 500).');
  });
});
