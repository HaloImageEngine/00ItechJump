import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { API_ORIGIN } from '../config/api-origin';
import { AuditService, AuditLogRequest, AuditLogResponse } from './audit.service';

describe('AuditService', () => {
  let service: AuditService;
  let httpMock: HttpTestingController;
  const apiUrl = `${API_ORIGIN}/api/ITechJump/Audit/InsertAuditLog`;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [AuditService]
    });
    service = TestBed.inject(AuditService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should insert audit log', () => {
    const mockRequest: AuditLogRequest = {
      Activity_Id: 0,
      Activity_Date: '2026-02-04T13:25:04.113Z',
      UserAlias: 'HaloLL19',
      UserId: 0,
      ActivityCode: 'LOGIN',
      ActivityMessage: 'User logged in',
      ProgramSend: 'ITechJump',
      Pages: 'main-page',
      Site: 'mobile'
    };

    const mockResponse: AuditLogResponse = {
      auditID: 1001
    };

    service.insertAuditLog(mockRequest).subscribe(response => {
      expect(response).toEqual(mockResponse);
      expect(response.auditID).toBe(1001);
    });

    const req = httpMock.expectOne(apiUrl);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(mockRequest);
    expect(req.request.headers.get('Content-Type')).toBe('application/json');
    req.flush(mockResponse);
  });

  it('should create audit log with current timestamp', () => {
    const mockResponse: AuditLogResponse = {
      auditID: 1002
    };

    service.createAuditLog(
      'TestUser',
      123,
      'TEST_ACTION',
      'Test message',
      'TestProgram',
      'test-page',
      'mobile'
    ).subscribe(response => {
      expect(response.auditID).toBe(1002);
    });

    const req = httpMock.expectOne(apiUrl);
    expect(req.request.method).toBe('POST');
    expect(req.request.body.UserAlias).toBe('TestUser');
    expect(req.request.body.UserId).toBe(123);
    expect(req.request.body.ActivityCode).toBe('TEST_ACTION');
    req.flush(mockResponse);
  });
});
