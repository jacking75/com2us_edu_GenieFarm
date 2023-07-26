# GenieFarm

23년 지니어스 인턴 프로젝트. Fram 장르 게임 서버 개발

---

# TODO-LIST

완료한 작업 : ✅

- **계정 기능**

| 기능                                         | 완료 여부 |
| -------------------------------------------- | --------- |
| [로그인](#로그인)                            | ✅        |
| [유저 등록](#클라이언트의-첫-게임-서버-접속) | ✅        |
| [게임 데이터 로드](#게임-데이터-로드)        | ✅        |
| [로그아웃](#로그아웃)                        | ✅        |

- **출석 기능**

| 기능                              | 완료 여부 |
| --------------------------------- | --------- |
| [출석 정보 조회](#출석-정보-조회) | ✅        |
| [출석 체크](#출석-체크)           | ✅        |

- **우편 기능**

| 기능                                            | 완료 여부 |
| ----------------------------------------------- | --------- |
| [우편함 페이지 별 조회](#우편함-페이지-별-조회) | ✅        |
| [개별 우편 조회](#개별-우편-조회)               | ⬜        |
| 우편 아이템 수령                                | ⬜        |
| 우편 삭제                                       | ⬜        |
| 우편 발송                                       | ⬜        |

- **아이템 기능**

| 기능                       | 완료 여부 |
| -------------------------- | --------- |
| 소유 아이템 페이지 별 조회 | ⬜        |

- **경매장 기능**

| 기능               | 완료 여부 |
| ------------------ | --------- |
| 경매장 아이템 검색 | ⬜        |
| 아이템 등록        | ⬜        |
| 등록 취소          | ⬜        |
| 입찰               | ⬜        |
| 즉시구매           | ⬜        |

- **친구 기능**

| 기능           | 완료 여부 |
| -------------- | --------- |
| 친구 목록 조회 | ⬜        |
| 친구 정보 조회 | ⬜        |
| 친구 검색      | ⬜        |
| 친구 요청      | ⬜        |
| 친구 요청 수락 | ⬜        |
| 친구 요청 거절 | ⬜        |
| 친구 삭제      | ⬜        |

---

## 클라이언트의 첫 게임 서버 접속

### (게임 데이터 생성)

(AuthCheckController.Create)

#### 컨텐츠 설명

- Hive 인증 데이터와 닉네임을 이용해 게임 데이터 생성을 시도합니다.

#### 로직

1. 클라이언트가 하이브 서버로부터 인증 받아오기
2. 클라이언트가 인증 정보로 서버에 게임 데이터 생성 요청
   - 최초 1회만 가능
3. 서버가 앱 버전, 게임 데이터 버전 확인
   - 미들웨어에서 수행
4. 서버가 하이브 서버로부터 클라이언트 정보에 대해 인증 확인 받기
5. 서버 DB에 플레이어 ID로 된 데이터가 존재하는지 확인
6. 기본 게임 데이터 생성
   - 기본 유저 정보 (닉네임도 확인)
   - 출석부 정보
   - 농장 기본 정보
   - 유저 아이템 정보

#### 클라이언트 → 서버 전송 데이터

| 종류                  | 설명                   |
| --------------------- | ---------------------- |
| Player ID             | 하이브 서버로부터 받음 |
| 인증 토큰             | 하이브 서버로부터 받음 |
| 닉네임                |                        |
| 앱 버전 정보          |                        |
| 게임 데이터 버전 정보 |                        |

#### 요청 및 응답 예시

- 요청 예시

```
POST http://localhost:11500/api/auth/create
Content-Type: application/json

{
    "PlayerID" : "test06",
    "AuthToken" : "P2H95LNF6NT8UC",
    "AppVersion" : "0.1",
    "MasterDataVersion" : "0.1",
    "Nickname" : "genie"
}
```

- 응답 예시

```
{
    "result": 0
}
```

---

## 로그인

(AuthCheckController.Login)

#### 컨텐츠 설명

- Hive 인증 데이터를 이용해 로그인합니다.

#### 로직

1. 클라이언트가 하이브 서버로부터 받은 인증 정보를 서버로 전송
2. 앱 버전 및 마스터데이터 버전 검증
   - 미들웨어에서 수행
3. 게임 서버가 해당 인증 정보에 대해 하이브 서버로 인증 요청
4. 게임 서버가 토큰을 생성해서 Redis에 저장
5. 게임 데이터 로드
   - 기본 유저 정보
   - 출석부 정보
   - 농장 기본 정보
6. 최종 로그인 시간 갱신
   - 유저가 친구 목록에서 최종 로그인 시간을 확인하고, 친구 목록을 관리할 수 있도록 갱신한다.

#### 클라이언트 → 서버 전송 데이터

| 종류                  | 설명                   |
| --------------------- | ---------------------- |
| Player ID             | 하이브 서버로부터 받음 |
| 인증 토큰             | 하이브 서버로부터 받음 |
| 앱 버전 정보          |                        |
| 게임 데이터 버전 정보 |                        |

#### 요청 및 응답 예시

- 요청 예시

```
POST http://localhost:11500/api/auth/login
Content-Type: application/json

{
    "PlayerID" : "test06",
    "AuthToken" : "P2H95LNF6NT8UC",
    "AppVersion" : "0.1",
    "MasterDataVersion" : "0.1"
}
```

- 응답 예시

```
{
    "defaultData": {
        "userData": {
            "userId": 99,
            "playerId": "test06",
            "nickname": "genie",
            "lastLoginAt": "2023-07-21T15:17:11"
        },
        "attendData": {
            "userId": 99,
            "attendanceCount": 0,
            "lastAttendance": "0001-01-01T00:00:00",
            "purchasedPass": false,
            "passEndDate": "0001-01-01T00:00:00"
        },
        "farmInfoData": {
            "userId": 99,
            "farmLevel": 1,
            "farmExp": 0,
            "money": 3000,
            "maxStorage": 100,
            "love": 5
        }
    },
    "authToken": "8fzg1yu71r8g3u77mbe4rj7tv",
    "result": 0
}
```

---

## 게임 데이터 로드

(LoadDataController.LoadDefaultData)

#### 컨텐츠 설명

- 게임 기본 데이터(기본 유저 정보, 출석부 정보, 농장 기본 정보)를 로드합니다.

#### 로직

1. 로그인한 클라이언트가 게임 기본 데이터 요청
2. 앱 버전 및 마스터데이터 버전 검증
3. 토큰 검증
   - 여기까지 미들웨어에서 수행
4. 게임 데이터 로드
   - 기본 유저 정보
   - 출석부 정보
   - 농장 기본 정보

클라이언트 → 서버 전송 데이터

| 종류                  | 설명                             |
| --------------------- | -------------------------------- |
| User ID               | 로그인 시에 게임 서버로부터 받음 |
| 인증 토큰             | 로그인 시에 게임 서버로부터 받음 |
| 앱 버전 정보          |                                  |
| 게임 데이터 버전 정보 |                                  |

#### 요청 및 응답 예시

- 요청 예시

```
POST http://localhost:11500/api/load/defaultdata
Content-Type: application/json

{
    "UserID" : 99,
    "AuthToken" : "8fzg1yu71r8g3u77mbe4rj7tv",
    "AppVersion" : "0.1",
    "MasterDataVersion" : "0.1"
}
```

- 응답 예시

```
{
    "result": 0
}
```

---

## 로그아웃

(AuthCheckController.Logout)

#### 컨텐츠 설명

- 유저의 토큰을 Redis에서 제거해 로그아웃 처리합니다.

#### 로직

1. 로그인한 클라이언트가 로그아웃 요청
2. 앱 버전 및 마스터데이터 버전 검증
3. 토큰 검증
   - 여기까지 미들웨어에서 수행
4. Redis에 저장된 토큰 삭제

클라이언트 → 서버 전송 데이터

| 종류                  | 설명                             |
| --------------------- | -------------------------------- |
| User ID               | 로그인 시에 게임 서버로부터 받음 |
| 인증 토큰             | 로그인 시에 게임 서버로부터 받음 |
| 앱 버전 정보          |                                  |
| 게임 데이터 버전 정보 |                                  |

#### 요청 및 응답 예시

- 요청 예시

```
POST http://localhost:11500/api/auth/logout
Content-Type: application/json

{
    "UserID" : 99,
    "AuthToken" : "8fzg1yu71r8g3u77mbe4rj7tv",
    "AppVersion" : "0.1",
    "MasterDataVersion" : "0.1"
}
```

- 응답 예시

```
{
    "result": 0
}
```

---

## 출석 정보 조회

(LoadDataController.LoadAttendData)

#### 컨텐츠 설명

- 현재 유저의 **출석 데이터**와 **출석 보상 리스트**를 로드합니다.
  - 출석 데이터 : 누적 출석수, 마지막 출석날짜

#### 로직

1. 로그인한 클라이언트가 출석 정보 조회 요청
2. 앱 버전 및 마스터데이터 버전 검증
3. 토큰 검증
   - 여기까지 미들웨어에서 수행
4. 마스터DB에서 출석 보상 리스트 가져옴
5. 유저의 출석 데이터를 게임DB에서 로드
6. 클라이언트에게 전송

#### 클라이언트 → 서버 전송 데이터

| 종류                  | 설명                             |
| --------------------- | -------------------------------- |
| User ID               | 로그인 시에 게임 서버로부터 받음 |
| 인증 토큰             | 로그인 시에 게임 서버로부터 받음 |
| 앱 버전 정보          |                                  |
| 게임 데이터 버전 정보 |                                  |

#### 요청 및 응답 예시

- 요청 예시

```
POST http://localhost:11500/api/attend
Content-Type: application/json

{
    "UserID" : 99,
    "AuthToken" : "8fzg1yu71r8g3u77mbe4rj7tv",
    "AppVersion" : "0.1",
    "MasterDataVersion" : "0.1"
}
```

- 응답 예시

```
{
    "monthlyRewardList": [
        {
            "day": 1,
            "itemCode": 1,
            "money": 0,
            "count": 30
        },
        {
            "day": 2,
            "itemCode": 2,
            "money": 0,
            "count": 30
        }, ... (중략)
    ],
    "attendData": {
        "userId": 99,
        "attendanceCount": 1,
        "lastAttendance": "2023-07-21T15:17:59",
        "passEndDate": "0001-01-01T00:00:00"
    },
    "result": 0
}
```

---

## 출석 체크

(AttendanceController.Attend)

#### 설명

- 현재 시간으로 출석 처리하고, 우편함으로 보상을 지급받습니다.
- 연속 출석이 아닌 **누적 출석수**로 보상을 지급합니다.
  - ex. 7월 1일, 7월 3일에 출석했다면 7월 4일에는 3일차 보상을 받음
- 보상은 1일 단위로 있으며, 최대 누적 일수에 도달하면 1일차로 돌아옵니다.

#### 로직

1. 로그인한 클라이언트가 출석 체크 요청
2. 앱 버전 및 마스터데이터 버전 검증
3. 토큰 검증
   - 여기까지 미들웨어에서 수행
4. 출석 데이터 로드
5. 출석 가능한지 확인
   - 하루에 1번만 가능
6. 출석 체크
   - 출석 정보 갱신
   - 보상 아이템을 우편함으로 지급(월간 구독제 이용 중이라면, 보상을 2배로 제공)

#### 클라이언트 → 서버 전송 데이터

| 종류                  | 설명                             |
| --------------------- | -------------------------------- |
| User ID               | 로그인 시에 게임 서버로부터 받음 |
| 인증 토큰             | 로그인 시에 게임 서버로부터 받음 |
| 앱 버전 정보          |                                  |
| 게임 데이터 버전 정보 |                                  |

#### 요청 및 응답 예시

- 요청 예시

```
{
    "UserID" : 99,
    "AuthToken" : "8fzg1yu71r8g3u77mbe4rj7tv",
    "AppVersion" : "0.1",
    "MasterDataVersion" : "0.1"
}
```

- 응답 예시

```
{
    "result": 0
}
```

---

## 우편함 페이지 별 조회

(MailController.LoadMailListByPage)

#### 설명

- 유저에게 도착한 우편을 페이지 단위로 조회합니다.
  - 페이지 당 우편 개수는 마스터DB에 정의된 값을 사용합니다. (현재 10개)
  - 우편 보관일이 지났거나, 삭제된 우편은 가져오지 않습니다.
- 페이지 번호는 1부터 시작합니다.

#### 로직

1. 로그인한 클라이언트가 페이지 번호와 함께 우편함 페이지 별 조회 요청
2. 앱 버전 및 마스터데이터 버전 검증
3. 토큰 검증
   - 여기까지 미들웨어에서 수행
4. 해당하는 페이지의 우편 10개를 로드 및 반환

#### 클라이언트 → 서버 전송 데이터

| 종류                  | 설명                             |
| --------------------- | -------------------------------- |
| User ID               | 로그인 시에 게임 서버로부터 받음 |
| 인증 토큰             | 로그인 시에 게임 서버로부터 받음 |
| 앱 버전 정보          |                                  |
| 게임 데이터 버전 정보 |                                  |
| 페이지 번호           |                                  |

#### 요청 및 응답 예시

- 요청 예시

```
{
    "UserID" : 99,
    "AuthToken" : "d736woy2cbe23c1ckgrdrlgo5",
    "AppVersion" : "0.1",
    "MasterDataVersion" : "0.1",
    "Page" : 1
}
```

- 응답 예시

```
{
    "mailList": [
        {
            "mailId": 107,
            "receiverId": 99,
            "senderId": 0,
            "title": "출석 보상 지급",
            "content": "1일차 출석 보상입니다.",
            "obtainedAt": "2023-07-21T15:17:59",
            "expiredAt": "2023-07-28T15:17:59",
            "isRead": false,
            "isDeleted": false,
            "isReceived": false,
            "itemCode": 1,
            "itemCount": 30,
            "money": 0
        },
        {
            "mailId": 109,
            "receiverId": 99,
            "senderId": 0,
            "title": "출석 보상 지급",
            "content": "2일차 출석 보상입니다.",
            "obtainedAt": "2023-07-24T19:05:17",
            "expiredAt": "2023-07-31T19:05:17",
            "isRead": false,
            "isDeleted": false,
            "isReceived": false,
            "itemCode": 2,
            "itemCount": 30,
            "money": 0
        }
    ],
    "result": 0
}
```

---

## 개별 우편 조회

(MailController.LoadMail)

#### 설명

- 클라이언트가 요청한 특정 ID를 가진 메일 데이터를 조회합니다.
- 아이템이 첨부되어 있다면, 아이템 정보도 함께 반환합니다.
- 한 번 조회한 메일은 읽음 처리가 됩니다(DB에 반영됨).

#### 로직

1. 로그인한 클라이언트가 메일ID와 함께 개별 우편 조회 요청
2. 앱 버전 및 마스터데이터 버전 검증
3. 토큰 검증
   - 여기까지 미들웨어에서 수행
4. 메일 데이터 로드
   - 메일 ID가 일치하고 메일 수신자 ID가 요청 유저 ID와 일치하는 행을 반환
5. (아이템이 첨부된 경우) 마스터DB에서 가져온 아이템 데이터를 추가

#### 클라이언트 → 서버 전송 데이터

| 종류                  | 설명                             |
| --------------------- | -------------------------------- |
| User ID               | 로그인 시에 게임 서버로부터 받음 |
| 인증 토큰             | 로그인 시에 게임 서버로부터 받음 |
| 앱 버전 정보          |                                  |
| 게임 데이터 버전 정보 |                                  |
| 메일 ID               |                                  |

#### 요청 및 응답 예시

- 요청 예시

```
{
    "UserID" : 96,
    "AuthToken" : "8e3vy5bg96on2p2a59lp4iryk",
    "AppVersion" : "0.1",
    "MasterDataVersion" : "0.1",
    "MailID" : 102
}
```

- 응답 예시

```
{
    "mail": {
        "itemAttribute": {
            "code": 1,
            "typeCode": 1,
            "name": "벼",
            "sellPrice": 2,
            "buyPrice": 2,
            "desc": "싱싱한 벼이다."
        },
        "mailId": 144,
        "receiverId": 112,
        "senderId": 0,
        "title": "출석 보상 지급",
        "content": "1일차 출석 보상입니다.",
        "obtainedAt": "2023-07-26T12:11:48",
        "expiredAt": "2023-08-02T12:11:48",
        "isRead": true,
        "isDeleted": false,
        "isReceived": false,
        "itemCode": 1,
        "itemCount": 30,
        "money": 0
    },
    "result": 0
}
```

---

## 우편 아이템 수령

(MailController.ReceiveMailItem)

#### 설명

- 클라이언트가 요청한 메일ID에 있는 아이템을 수령 처리하고 실제 지급합니다.

#### 로직

1. 로그인한 클라이언트가 메일ID와 함께 개별 우편 아이템 수령 요청
2. 앱 버전 및 마스터데이터 버전 검증
3. 토큰 검증
   - 여기까지 미들웨어에서 수행
4. 해당 메일에 아이템이나 재화가 있는지 확인 후, 수령 여부를 변경함
   - 메일 ID, 메일 수신자 ID가 일치하는 행을 찾아 `IsReceived`를 `true` 로 Update
5. 아이템 지급 처리
   - `user_item` 테이블에 아이템 데이터를 Insert
6. 재화 지급 처리
   - `farm_info` 테이블의 Money값을 증가

#### 클라이언트 → 서버 전송 데이터

| 종류                  | 설명                             |
| --------------------- | -------------------------------- |
| User ID               | 로그인 시에 게임 서버로부터 받음 |
| 인증 토큰             | 로그인 시에 게임 서버로부터 받음 |
| 앱 버전 정보          |                                  |
| 게임 데이터 버전 정보 |                                  |
| 메일 ID               |                                  |
| 아이템 Code           |                                  |
| 아이템 Count          |                                  |
| 재화                  |                                  |

#### 요청 및 응답 예시

- 요청 예시

```
{
    "UserID" : 96,
    "AuthToken" : "8e3vy5bg96on2p2a59lp4iryk",
    "AppVersion" : "0.1",
    "MasterDataVersion" : "0.1",
    "MailID" : 102
}
```

- 응답 예시

```
{
    "mail": {
        "itemAttribute": {
            "code": 1,
            "typeCode": 1,
            "name": "벼",
            "sellPrice": 2,
            "buyPrice": 2,
            "desc": "싱싱한 벼이다."
        },
        "itemCount": 30,
        "mailId": 102,
        "receiverId": 96,
        "senderId": 0,
        "title": "출석 보상 지급",
        "content": "1일차 출석 보상입니다.",
        "obtainedAt": "2023-07-21T15:06:52",
        "expiredAt": "2023-07-28T15:06:52",
        "isRead": false,
        "isDeleted": false,
        "itemId": 97,
        "isReceived": false,
        "money": 0
    },
    "result": 0
}
```

---

---

## 우편

우편 탭을 기능별로 구분하여 사용한다.

- 공지(알림) / 일반 유저와의 우편 / 길드 우편 등

우편함은 페이지 형식으로 조회한다.

- 우편함을 열 때 조회되는 페이지는 1페이지로 고정된다.
- 한 페이지에 표시되는 우편 개수에는 제한이 있으며, 유저가 연 페이지에 속하는 우편 데이터만 로딩한다.
- 한번 로딩한 우편 데이터는 클라이언트가 캐싱하여 우편함을 닫을 때까지 유지한다.
- 우편은 수신일자 순으로 정렬한다.

우편 목록에서 우편 확인 여부를 음영으로, 아이템 첨부 여부를 아이콘으로 확인할 수 있으며, 우편 상세 확인시 우편 내용 및 첨부된 아이템 목록을 확인할 수 있다.

첨부 아이템 수령은 일괄 수령만 가능하도록 하며, 유저의 우편 전체의 아이템을 일괄 수령할 수도 있다.

아이템이 첨부되어있는 우편에는 유효기간이 존재하며, 유효기간이 지날 경우 아이템을 수령할 수 없다.

- 이 때 아이템이 타 유저가 보낸 것이었다면 반송한다.

우편 삭제시에는 물리적 삭제가 아닌 논리적 삭제를 사용한다.

- 아직 읽지 않은 우편 삭제시에는 경고 메시지를 발생시킨다
- 우편을 읽었으나 첨부된 아이템을 수령하지 않은 경우에도 경고 메시지를 발생시킨다.

우편 전송시에는 전송 상대를 우선 확인한다.

경매장에서 낙찰에 성공한 아이템, 출석 보상, 친구로부터 받은 선물, 창고가 가득 차 보관하지 못한 수확물 등의 아이템들을 우편으로 지급한다.

---

- 조회
  - 1페이지에 20개씩 표시된다.
  - 이미 열어본 페이지에 대해서는 클라이언트가 캐싱한다.
  - 첨부된 아이템의 경우 유효기간이 존재한다.
- 수령
- 삭제
- 발송

## 인앱결제

클라이언트가 스토어로부터 결제 후, 임의의 결제 데이터(영수증)를 서버로 전송한다고 가정한다.

영수증에는 구매ID, 구매 물품, 구매 수량 등의 정보가 포함되어있다고 가정한다.

영수증에 대해서는 구매ID에 대한 중복 요청만 검증한다.

---

- 결제 검증
  - 검증된 결제건에 대한 아이템 보상은 우편으로 지급한다.

## 경매장

유저가 아이템을 올려 24시간동안 경매를 진행할 수 있다. 올릴 때 최소가와 즉시구매가를 설정한다.

24시간이 되는 시점까지 즉시구매가 이루어지지 않았다면, 가장 높은 금액을 등록한 유저의 우편함에 아이템을 지급한다.

- 유찰될 경우에는 아이템을 우편으로 반송한다.
- 즉시구매를 수행하면 즉시 구매자의 우편함으로 지급한다.

아이템의 종류에 따라 거래 가능 / 불가능한 아이템이 구분되어있다.

유저가 카테고리를 선택하거나 특정 아이템을 검색했을 시에 데이터를 로딩한다.

- 로딩한 데이터들은 캐싱하여 사용한다.
- 단, 캐싱한 데이터는 실시간 데이터와 차이가 있을 수 있으므로 실제 입찰 및 즉시구매시에는 유효한 품목인지 재검증하고, 해당 품목의 정보를 다시 로딩한다.
- 경매장을 닫거나, 새로고침하거나, 다른 데이터를 검색할 시에는 기존 캐싱 데이터를 삭제한다.

경매장은 기본 등록순으로 정렬하며, 즉시구매가 순, 남은 경매시간 순으로도 정렬이 가능하다.

아이템 검색은 이름으로 할 수 있으며, 최소가 및 평균가를 표시한다.

거래할 수 있는 아이템 종류는 생산물, 음식 아이템으로 한다.

---

- 아이템 검색
  - 조건 설정(정렬, 정렬기준, 페이지, …)
- 입찰
- 즉시구매

## 친구

다른 유저와 친구 관계를 맺을 수 있다.

하루에 한 번씩 랜덤하게 선물을 전송할 수 있다. (골드, 생산물, 경험치)

친구 검색은 닉네임으로 할 수 있다.

친구 추가는 팔로우 개념으로 별도의 수락이 필요없다. 원치 않는 유저가 팔로우했다면 차단할 수는 있다.

- 자신이 팔로우 할 수 있는 수 / 팔로우 당할 수 있는 수의 최대치가 존재한다.
- 차단 목록(블랙리스트)가 존재한다.

친구 농장에 놀러가면 방명록에 글을 쓸 수 있다.

- 방명록도 우편과 같이 페이지 형식을 사용한다.

추천 친구를 확인할 수 있다.

- 기본적으로는 랜덤한 유저가 추천된다.
- 일부는 비슷한 레벨의 유저 중에서 추천되도록 한다.

---

- (내) 친구 목록 조회
- 친구 검색
- 친구 추가
- 친구 차단
- 친구 정보 조회 (농장 레벨, …)
- 친구에게 선물 전송
- 친구 농장 놀러가기 (친구 농장 배치 정보 로드)
- 친구 방명록 로딩 및 글 남기기
- 추천 친구

## 채팅

로그인 시 자동으로 임의의 채널에 입장된다.

- 채널 전체 인원의 70% 미만이 차있는 채널이 존재할 경우 해당 채널에 우선 입장된다.
- 위 조건에 해당하는 채널이 다수일 경우 그중 가장 인원이 많은 채널에 우선 입장된다.
- 모든 채널의 인원수가 전체의 70% 이상일 경우 가장 인원이 적은 채널에 우선 입장된다.
- 우선순위가 동일한 채널들이 존재하는 경우 가장 번호가 작은 채널에 우선 입장된다.

유저가 채널을 임의로 변경할 수 있다.

- 변경할 채널의 인원이 가득 차있는 경우에는 불가능하다.
- 변경된 채널의 이전 대화기록은 확인할 수 없다.

모든 채널이 가득 차있는 경우에는… 채널을 늘린다?

Redis를 사용해 구현한다.

GM 계정일 경우 전 채널에 공지를 전송할 수 있다. (일반 채팅은 불가능하다.)

---

- 채널 입장(로그인과 동시에 임의로 배정)
- 채널 변경
- 송신
- 수신
- GM 공지 메시지

## 퀘스트 및 업적

퀘스트마다 해금 조건이 있다(레벨, 아이템 보유 개수, 선행 퀘스트 완료 여부).

해금된 퀘스트는 진행 가능한 퀘스트로 분류된다.

이중 수락한 퀘스트가 진행중인 퀘스트로 분류된다.

이미 완료한 퀘스트는 완료한 퀘스트로 분류된다.

퀘스트를 완료하면 EXP 혹은 아이템을 보상으로 얻는다.

도감 시스템이 있어, 동물 또는 식물에 대한 도감(아이템을 획득한 적이 있다면 체크됨)이 일정 부분 완료될 때마다 EXP를 얻고 기본 능력치를 상승시킬 수 있다.

---

- 퀘스트 목록 조회 (진행 가능한 퀘스트, 진행중인 퀘스트, 완료한 퀘스트)
    <aside>
    💡 접속 후 퀘스트 목록 최초 열람시(캐싱 데이터가 없을 시) 클라가 서버에게 (진행 가능 / 진행중 / 완료) 탭에 해당하는 퀘스트 목록을 요청한다.
    목록은 페이지 단위로 요청한다.
    
    </aside>

- 퀘스트 완료
- 퀘스트 포기
- 퀘스트 수락
    <aside>
    💡 완료, 포기, 수락하여 (진행 가능 / 진행중 / 완료) 퀘스트 목록에 변동사항이 생기면 클라는 캐싱해둔 데이터를 무효화하고 1페이지부터 재요청하여 다시 캐싱한다.
    
    </aside>

- 도감 시스템

  - 동물 / 식물에 대한 도감을 일정 부분 완료할 때마다 기본 능력치 상승
    <aside>
    💡 최초로 특정 식물을 재배한 적이 있거나, 동물을 구매한 적이 있는지 체크하는 플래그 변수를 둔다.
    식물 재배 / 동물 구매시에 해당 플래그를 true로 바꾼다.
    도감 완료 여부는 접속 시 유저 데이터 로드와 함께 체크하여 캐싱하고, 새로운 재배 및 구매 요청이 왔을 때, 도감 탭을 누를 때에도 갱신된다.

    </aside>

## 재배 및 사육

농작물은 가지고 있는 논밭 개수만큼 재배할 수 있다.

논밭의 개수는 특정 레벨마다 증가한다.

재배하고 일정 시간이 지나면 작물을 수확할 수 있다. 이 시간은 작물의 종류마다 다르다.

동물은 현재 해금된 사육 가능 마릿수만큼 사육할 수 있다.

사육 가능 마릿수는 특정 레벨마다, 동물 종류별로 증가한다.

먹이기나 쓰다듬기를 일정 시간마다 N회까지 반복할 수 있다. 해당 동작 수행 1회마다 동물의 경험치가 오른다.

동물 경험치가 다 차면 동물 레벨이 오른다.

동물 레벨이 오르면 생산 시간이 줄어들고 단위 당 생산량이 늘어난다.

동물을 판매하면 재화(골드)를 얻을 수 있다.

수확 완료하지 않으면 다음 생산물을 생산할 수 없다.

---

- 영토에 논밭(재배지역) 추가
    <aside>
    💡 클라가 식별자를 주면 이를 토대로 서버가 DB에 있는 **레벨 별 최대 재배지역 개수**와 **현재 보유한 재배지역 개수**를 비교하여 성공 및 실패를 판단한다.
    
    </aside>

- 작물 재배
    <aside>
    💡 클라가 식별자와 작물 종류, 영토 번호를 주면 이를 토대로 재배 가능한 작물인지 검증한다. (해당 작물을 재배할 수 있는 레벨인지, 해당 영토가 비어있는 상태인지)
    검증 완료되면 당시의 timestamp를 기반으로 생산 완료 시간을 계산해 DB에 저장한다.
    
    </aside>

- 수확
    <aside>
    💡 클라가 식별자와 영토 번호를 주면 해당 영토의 생산완료시간을 현재 시간과 비교하여 수확 가능한지 검증한다.
    수확 가능하다면, 창고에 칸이 있는지 확인하고,
    칸이 있다면 즉시 지급, 없다면 우편함으로 지급한다.
    
    </aside>

- 동물 분양
    <aside>
    💡 클라가 식별자와 동물 종류를 주면 이를 토대로 (현재 레벨에서) 사육 가능한지 검증한다.
    구매(분양)한 동물의 고유 식별자를 유저에게 전달한다.
    
    </aside>

- 동물 배치
    <aside>
    💡 동물을 구매한 순간이 아니라 맵에 배치한 순간부터 생산 완료 시간을 계산해 DB에 저장한다.
    
    </aside>

- 먹이 주기
- 쓰다듬어주기
    <aside>
    💡 레벨에 따라 일정량의 동물 경험치를 상승시킨다.
    생산 시간 단위마다 N회씩 반복 가능하며, N은 최대 5이다.
    경험치가 오르면 레벨업이 활성화된다.
    
    </aside>

- 동물 생산물 수확
    <aside>
    💡 클라가 식별자와 동물 식별자를 주면 해당 동물의 생산완료시간을 현재 시간과 비교하여 수확 가능한지 검증한다.
    수확 가능하다면, 창고에 칸이 있는지 확인하고,
    칸이 있다면 즉시 지급, 없다면 우편함으로 지급한다.
    
    </aside>

- 동물 레벨업
    <aside>
    💡 버튼을 눌러 동물 레벨업을 수행한다. 실제 경험치가 레벨업 가능한 경험치에 도달했는지 검증한다.
    
    </aside>

- 동물 판매
    <aside>
    💡 동물의 레벨에 따라 일정량의 골드를 지급하고, 해당 동물 데이터를 삭제한다.
    
    </aside>

## 건설

레벨에 따라 건설 가능한 건축물이 해금된다.

건축물마다 생산 시간이 있고, 이 시간마다 재료를 수확할 수 있다.

수확 완료하지 않으면 다음 생산물을 생산할 수 없다.

---

- 건설 시작
    <aside>
    💡 유저가 해당 건물을 건설할 수 있는지 검증 후 건설 완료 시간을 계산해 저장한다.
    
    </aside>

- 건물 레벨업
    <aside>
    💡 골드와 아이템을 사용해 건물을 레벨업한다. 유저가 실제로 충분한 골드와 아이템을 가지고 있는지 검증한다.
    
    </aside>

- 건설 완료
    <aside>
    💡 해당 건물이 건설 완료 시간에 도달했는지 현재 시간과 비교하여 검증 후 응답한다.
    
    </aside>

- 생산물 수확
    <aside>
    💡 생산 완료 시간에 도달했는지 현재 시간과 비교하여 검증한다.
    창고에 자리가 있는지 확인 후, 있으면 창고로, 없으면 우편함으로 지급한다.
    
    </aside>

## 강화

유저 개인의 능력치를 강화할 수 있다.

강화에는 특정 재료들과 골드가 필요하다.

능력치에는

1. 수확 (성공/대성공) 능력
2. 생산 능력 (→ 시간 감소)
3. 건설 능력 (→ 건설 시간 감소)
4. 경험치 획득률

이 있다.

레벨이 높아질수록 강화 성공 확률이 떨어진다. 최대 레벨은 10이다.

---

- 능력치 강화
    <aside>
    💡 레벨 별 강화 확률을 두고 이 확률에 따라 능력치를 강화한다.
    사용자가 충분한 재화와 골드를 가지고있는지 검증한다.
    
    </aside>

## 길드

길드를 만들고 가입할 수 있다.

가입은 길드마스터 혹은 부마스터가 승인해줘야 한다.

삭제할 때에는 길드원이 한 명(길드마스터)만 있어야 한다.

길드 농장에서 농작물을 키워 하루에 한 번씩 보상을 얻을 수 있다.

길드원이 길드 농작물에 물을 주거나, 강화 아이템을 사용하면 명성치가 오른다.

명성치가 일정 수준에 도달하면 길드 레벨이 오르고, 길드 농작물로 얻을 수 있는 보상도 달라진다.

---

- 길드 검색
    <aside>
    💡 길드명으로 검색한다. 길드가 등록된 순으로 20개씩 페이징 처리하여 제공한다.
    
    </aside>

- 길드 랭킹 조회 (1~100위까지, 페이징 처리)
    <aside>
    💡 랭킹은 길드 레벨 순, 길드 명성치 순으로 정렬하여 제공한다. 20개씩 페이징 처리한다.
    
    </aside>

- 길드 생성
    <aside>
    💡 일정량의 골드를 지불하여 길드를 생성한다.
    유저가 길드에 속해있지 않은지, 길드명이 중복되지 않았는지, 재화가 충분한지 체크한다.
    
    </aside>

- 길드 가입
    <aside>
    💡 유저가 어떤 길드에도 속해있지 않은지, 이미 가입 신청한 길드가 아닌지 확인 후 가입 신청 테이블에 새로 insert한다.
    가입 신청 시 짧은 메시지를 작성할 수 있다.
    
    </aside>

- 길드 가입 요청 조회
    <aside>
    💡 해당 API를 요청한 유저가 부마스터 이상의 직위인지 확인해야 한다.
    오래된 시간 순으로 정렬하여 20개씩 페이징 처리 후 제공한다. (변동이 있기 전까지 클라에서 캐싱한다.)
    가입 신청 유저의 농장레벨, 유저의 최근 접속 시간, 가입 메시지를 제공한다.
    
    </aside>

- 길드 가입 수락 및 거절
    <aside>
    💡 길드 가입 신청에 대해 수락 및 거절을 한다. 수락할 경우, 해당 유저가 어떤 길드에도 속해있지 않은지 유효성 검증 후 수락한다.
    해당 API 수행 이후 길드 가입 요청을 재로드한다.
    
    </aside>

- 길드마스터 위임
    <aside>
    💡 요청을 보낸 유저가 마스터인지 확인한다.
    특정 유저의 직위를 마스터로 변경한다.
    
    </aside>

- 길드부마스터 임명
    <aside>
    💡 요청을 보낸 유저가 마스터인지 확인한다.
    특정 유저의 직위를 부마스터로 변경한다.
    
    </aside>

- 길드부마스터 해임
    <aside>
    💡 요청을 보낸 유저가 마스터인지 확인한다.
    특정 유저의 직위를 일반 길드원으로 변경한다.
    
    </aside>

- 길드원 강퇴
    <aside>
    💡 요청을 보낸 유저가 부마스터 이상의 직위인지 확인해야 한다.
    특정 유저를 길드 멤버 리스트에서 삭제한다.
    
    </aside>

- 길드 삭제
    <aside>
    💡 요청을 보낸 유저가 마스터인지 확인한다.
    길드원이 마스터 1명뿐인지 확인 후 길드를 삭제한다.
    
    </aside>

- 길드 정보 조회 (명성치, 길드원 수, 길드 레벨)
- 길드원 조회 (유저 정보 및 최근 접속일 표시)
    <aside>
    💡 요청을 보낸 유저가 해당 길드원인지 확인한다.
    
    </aside>

- 길드 게시판
    <aside>
    💡 요청을 보낸 유저가 해당 길드원인지 확인한다.
    
    </aside>

- 길드 농작물 물주기
    <aside>
    💡 요청을 보낸 유저가 해당 길드원인지 확인한다.
    
    </aside>

## 요리

레시피 상점에서 골드로 레시피를 구매할 수 있다.

생산물을 재료로 하여 요리를 제작할 수 있다.

제작된 음식을 사용하면 각종 버프를 제공한다.

---

- 레시피 구매
    <aside>
    💡 이미 배운 적 있는 레시피라면 구매해도 사용할 수 없다. (이에 대한 플래그 변수를 둬야한다)
    
    </aside>

- 요리 제작
    <aside>
    💡 충분한 재료를 가지고 있는지 확인 후 아이템을 지급한다.
    
    </aside>

- 음식 사용 (동물 생산시간 감소 버프 등…)
    <aside>
    💡 음식을 동물에게 먹인다.
    동물과 아이템을 실제로 보유하고 있는지 확인 후, 아이템 종류에 맞는 버프를 적용한다.
    
    </aside>

## 아이템 관련

아이템을 저장할 수 있는 창고에는 기본 100개의 칸이 있다.

동일 생산물은 한 칸에 100개씩 저장할 수 있다.

골드를 사용해 창고를 50칸씩 확장할 수 있으며, 최대 300칸까지 가능하다.

---

- 창고 확장
    <aside>
    💡 확장에 필요한 충분한 골드를 가지고 있는지 확인 후 창고 칸을 늘린다.
    
    </aside>

## 농장 정보

- 농장 레벨
- 농장 경험치
- 설치 가능한 필드아이템 개수
  - 논/밭
  - 동물(종류별 카운트)
  - 건물
- 농작물/동물/건물 배치 정보
  - 섬 크기
  - 농작물/동물/건물 종류와 배치 위치
- 소유 아이템 정보
  - 골드
  - 아이템
- 기본 능력치 정보
  - 수확 (성공/대성공) 능력 (→ %가 높다면 그 확률에 따라 수확에 대성공하여 더 많은 생산물을 얻을 수 있음)
  - 생산 능력 (→ %가 높을수록 생산시간 감소함)
  - 건설 능력 (→ %에 따라 건설 시간 감소)
  - 경험치 획득률 (→ %에 따라 경험치 획득률 증가)
