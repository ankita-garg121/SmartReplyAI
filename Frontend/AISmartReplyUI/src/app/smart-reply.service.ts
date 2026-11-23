import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class SmartReplyService {

  private apiUrl = 'https://localhost:7032/api/SmartReply/generate';

  constructor(private http: HttpClient) { }

  generateReply(request: any): Observable<any> {
    return this.http.post<any>(this.apiUrl, request);
  }
}