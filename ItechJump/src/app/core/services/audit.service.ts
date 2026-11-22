import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';

import { API_ORIGIN } from '../config/api-origin';

export interface AuditLogRequest {
  Activity_Id: number;
  Activity_Date: string;
  UserAlias: string;
  UserId: number;
  ActivityCode: string;
  ActivityMessage: string;
  ProgramSend: string;
  Pages: string;
  Site: string;
}

export interface AuditLogResponse {
  auditID: number;
}

@Injectable({
  providedIn: 'root'
})
export class AuditService {
  private readonly apiUrl = `${API_ORIGIN}/api/ITechJump/Audit/InsertAuditLog`;

  constructor(private http: HttpClient) {}

  insertAuditLog(auditLog: AuditLogRequest): Observable<AuditLogResponse> {
    const headers = new HttpHeaders({
      'Content-Type': 'application/json'
    });

    return this.http.post<AuditLogResponse>(this.apiUrl, auditLog, { headers });
  }

  /**
   * Helper method to create an audit log with current timestamp
   */
  createAuditLog(
    userAlias: string,
    userId: number,
    activityCode: string,
    activityMessage: string,
    programSend: string,
    pages: string,
    site: string
  ): Observable<AuditLogResponse> {
    const auditLog: AuditLogRequest = {
      Activity_Id: 0,
      Activity_Date: new Date().toISOString(),
      UserAlias: userAlias,
      UserId: userId,
      ActivityCode: activityCode,
      ActivityMessage: activityMessage,
      ProgramSend: programSend,
      Pages: pages,
      Site: site
    };

    return this.insertAuditLog(auditLog);
  }
}
