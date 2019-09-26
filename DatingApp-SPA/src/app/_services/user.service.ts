import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { User } from '../_models/user';
import { Observable } from 'rxjs';
import { PaginatedResult } from '../_models/pagination';
import { map } from 'rxjs/operators';
import { Message } from '../_models/message';


// const httpOptions ={
//   headers: new HttpHeaders({
//     'Authorization': 'Bearer ' + localStorage.getItem('token')
//   })
// }
@Injectable({
  providedIn: 'root'
})
export class UserService {
  baseUrl = environment.apiUrl + 'users/';

constructor(private http: HttpClient) { }

// getUsers(): Observable<User[]> {
//   return this.http.get<User[]>(this.baseUrl);
// }

// Paginated list of  users
getUsers(page?, itemsPerPage?, userParams?, likesParam?): Observable<PaginatedResult<User[]>> {
  const paginatedResult: PaginatedResult<User[]> = new PaginatedResult<User[]>();

// pagination parameters
  let params = new HttpParams();
  if (page != null && itemsPerPage != null) {
    params = params.append('pageNumber', page);
    params = params.append('pageSize', itemsPerPage);
  }

  // filtering parameters
  if (userParams != null) {
    params = params.append('minAge', userParams.minAge);
    params = params.append('maxAge', userParams.maxAge);
    params = params.append('gender', userParams.gender);
    params = params.append('orderBy', userParams.orderBy);
  }

// to display the list of likees and likers
  if (likesParam === 'Likers') {
    params = params.append('likers', 'true');
  }

  if (likesParam === 'Likees') {
    params = params.append('likees', 'true');
  }
  return this.http.get<User[]>(this.baseUrl, {observe: 'response', params})
    .pipe(
      map(response => {
        paginatedResult.result = response.body;
        if (response.headers.get('Pagination') != null) {
          paginatedResult.pagination = JSON.parse(response.headers.get('Pagination'));
        }
        return paginatedResult;
      })
    );
}

getUser(id): Observable<User> {
  return this.http.get<User>(this.baseUrl + id);
}

updateUser(id: number, user: User) {
  return this.http.put(this.baseUrl + id, user);
}

setMainPhoto(userId: number, id: number) {
  return this.http.post(this.baseUrl + userId + '/photos/' + id + '/setMain', {});
}
deletePhoto(userId: number, id: number) {
  return this.http.delete(this.baseUrl + userId + '/photos/' + id );
}
sendLike(id: number, recipientId: number) {
  return this.http.post(this.baseUrl + id + '/like/' + recipientId, {});
}

getMessages(id: number, page?, itemsPerPage?, messageContainer?) {
 const paginatedResult: PaginatedResult<Message[]> = new PaginatedResult<Message[]>();

 let params = new HttpParams();
 params = params.append('MessageContainer', messageContainer);
if (page != null && itemsPerPage != null) {
  params = params.append('pageNumber', page);
  params = params.append('pageSize', itemsPerPage);
}

    return this.http.get<Message[]>(this.baseUrl + id + '/messages', {observe: 'response', params})
    .pipe(
      map(response => {
        paginatedResult.result = response.body;
        if (response.headers.get('Pagination') != null) {
          paginatedResult.pagination = JSON.parse(response.headers.get('Pagination'));
        }
        return paginatedResult;
      })
    );
}

// to get conversation between two users
getMessagesThread(id: number, recipientId: number) {
return this.http.get<Message[]>(this.baseUrl + id + '/messages/thread/' + recipientId);
}

sendMessage(id: number, message: Message) {
  return this.http.post(this.baseUrl + id + '/messages', message);
}

deleteMessage(id: number, userId: number) {
  return this.http.post(this.baseUrl + userId + '/messages/' + id, {});
}

markAsRead(id: number, messageId: number) {
  return this.http.post(this.baseUrl + id + '/messages/' + messageId + '/read/', {})
  .subscribe();
}
}
