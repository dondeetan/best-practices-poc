from datetime import datetime, timedelta, timezone

import jwt
from fastapi import Depends, HTTPException
from fastapi.security import HTTPAuthorizationCredentials, HTTPBasic, HTTPBasicCredentials, HTTPBearer
from passlib.context import CryptContext

from config import Settings, load_settings


settings = load_settings()

# Interface Segregation Principle: Basic authentication is used only to issue
# tokens, while Bearer authentication is used only to protect API resources.
basic_security = HTTPBasic()
bearer_security = HTTPBearer()
pwd_context = CryptContext(schemes=["bcrypt"], deprecated="auto")
users_db = {"userkey": pwd_context.hash(settings.userkey)}


def verify_basic(credentials: HTTPBasicCredentials = Depends(basic_security)) -> str:
    # Single Responsibility Principle: validates Basic credentials and returns
    # the authenticated subject without creating tokens or calling storage code.
    username = credentials.username
    password = credentials.password
    if username not in users_db or not pwd_context.verify(password, users_db[username]):
        raise HTTPException(
            status_code=401,
            detail="Invalid credentials",
            headers={"WWW-Authenticate": "Basic"},
        )
    return username


def create_access_token(sub: str, app_settings: Settings = settings) -> str:
    # Builder pattern: builds the JWT payload in a dedicated step before signing
    # it, keeping token construction separate from request handling.
    now = datetime.now(timezone.utc)
    payload = {
        "sub": sub,
        "iat": int(now.timestamp()),
        "exp": int((now + timedelta(minutes=app_settings.jwt_exp_minutes)).timestamp()),
    }
    return jwt.encode(payload, app_settings.jwt_secret, algorithm=app_settings.jwt_algorithm)


def get_current_user(token: HTTPAuthorizationCredentials = Depends(bearer_security)) -> str:
    # Chain of Responsibility style: FastAPI resolves this dependency before the
    # route runs, so invalid tokens stop the request early and consistently.
    try:
        payload = jwt.decode(token.credentials, settings.jwt_secret, algorithms=[settings.jwt_algorithm])
        sub = payload.get("sub")
        if not sub:
            raise HTTPException(status_code=401, detail="Invalid token: missing subject")
        return sub
    except jwt.ExpiredSignatureError:
        raise HTTPException(status_code=401, detail="Token expired")
    except jwt.InvalidTokenError:
        raise HTTPException(status_code=401, detail="Invalid token")
