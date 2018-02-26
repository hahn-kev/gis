import { Injectable } from '@angular/core';
import { User } from './user';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs/Observable';

@Injectable()
export class UserService {
  constructor(private http: HttpClient) {
  }

  getUsers(): Observable<User[]> {
    return this.http.get<User[]>('/api/user');
  }

  getUser(name: string): Observable<User> {
    return this.http.get<User>(`/api/user/${name}`);
  }

  getSelf(): Observable<User> {
    return this.http.get<User>('/api/user/self');
  }

  saveUser(user: User, password: string, isNewUser = false, isSelf = false): any {
    if (isNewUser) {
      return this.registerUser(user, password);
    } else if (isSelf) {
      return this.updateSelf(user, password);
    }
    return this.http.put(`/api/user/${user.id}`, {
      id: user.id,
      password: password,
      userName: user.userName,
      phoneNumber: user.phoneNumber,
      email: user.email,
      personId: user.personId
    }, {responseType: 'text'}).toPromise();
  }

  updateSelf(user: User, password: string): Promise<string> {
    return this.http.put('/api/user/self', {
      id: user.id,
      password: password,
      userName: user.userName,
      phoneNumber: user.phoneNumber,
      email: user.email
    }, {responseType: 'text'}).toPromise();
  }

  registerUser(user: User, password: string): Promise<Object> {
    return this.http.post<any>('/api/authenticate/register', {
      password: password,
      userName: user.userName,
      phoneNumber: user.phoneNumber,
      email: user.email,
      personId: user.personId
    }).toPromise();
  }

  grantRole(role: string, id: number): Promise<string> {
    return this.http.put(`/api/user/grant/${role}/${id}`, null, {responseType: 'text'}).toPromise();
  }

  revokeRole(role: string, id: number): Promise<string> {
    return this.http.put(`/api/user/revoke/${role}/${id}`, null, {responseType: 'text'}).toPromise();
  }

  deleteUser(userId: number): Promise<string> {
    return this.http.delete(`/api/user/${userId}`, {responseType: 'text'}).toPromise();
  }
}
