import uvicorn
from datetime import datetime, timedelta, timezone
from typing import Optional, List

from fastapi import FastAPI, HTTPException, Depends
from fastapi.security import HTTPBasic, HTTPBasicCredentials, HTTPBearer, HTTPAuthorizationCredentials
from passlib.context import CryptContext
from pydantic_settings import BaseSettings
import jwt  # PyJWT

from Entities.Cars import load_db, save_db, CarInput, CarOutput, TripOutput, TripInput


class Settings(BaseSettings):
    # Password for the single basic user (used only to mint a token)
    userkey: str

    # JWT settings
    jwt_secret: Optional[str] = None       # if not set, we'll derive from userkey
    jwt_algorithm: str = "HS256"
    jwt_exp_minutes: int = 60

    class Config:
        env_file = "appsettings"

settings = Settings()
if not settings.jwt_secret:
    # Derive a secret if none provided (still recommend setting JWT_SECRET in prod)
    settings.jwt_secret = f"derived::{settings.userkey}"


app = FastAPI(title="Car Sharing")
db: List[CarOutput] = load_db()

# Security primitives
basic_security = HTTPBasic()
bearer_security = HTTPBearer()

pwd_context = CryptContext(schemes=["bcrypt"], deprecated="auto")

# Simple in-memory user store: username -> hashed password
# Keeping your original intent: a single username "userkey" whose password is from env
users_db = {"userkey": pwd_context.hash(settings.userkey)}


# ---------- Auth helpers ----------
def verify_basic(credentials: HTTPBasicCredentials = Depends(basic_security)) -> str:
    username = credentials.username
    password = credentials.password
    if username not in users_db or not pwd_context.verify(password, users_db[username]):
        # Basic challenge so browsers/clients know to prompt
        raise HTTPException(status_code=401, detail="Invalid credentials", headers={"WWW-Authenticate": "Basic"})
    return username


def create_access_token(sub: str) -> str:
    now = datetime.now(timezone.utc)
    payload = {
        "sub": sub,
        "iat": int(now.timestamp()),
        "exp": int((now + timedelta(minutes=settings.jwt_exp_minutes)).timestamp()),
    }
    return jwt.encode(payload, settings.jwt_secret, algorithm=settings.jwt_algorithm)


def get_current_user(token: HTTPAuthorizationCredentials = Depends(bearer_security)) -> str:
    # token.credentials is the raw JWT
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


# ---------- Routes ----------
@app.post("/auth/token")
def login_and_issue_token(user: str = Depends(verify_basic)) -> dict:
    """
    Exchange valid Basic creds for a JWT. Use the returned Bearer token on subsequent calls.
    """
    token = create_access_token(sub=user)
    return {"access_token": token, "token_type": "bearer", "expires_in_minutes": settings.jwt_exp_minutes}


@app.get("/api/cars")
def get_cars(
    size: Optional[str] = None,
    doors: Optional[int] = None,
    user: str = Depends(get_current_user),
) -> list[CarOutput]:
    result = db
    if size:
        result = [car for car in result if car.size == size]
    if doors:
        result = [car for car in result if car.doors >= doors]
    return result


@app.get("/api/cars/{id}")
def car_by_id(id: int, user: str = Depends(get_current_user)) -> CarOutput:
    result = [car for car in db if car.id == id]
    if result:
        return result[0]
    else:
        raise HTTPException(status_code=404, detail=f"No car with id={id}.")


@app.post("/api/cars/")
def add_car(car: CarInput, user: str = Depends(get_current_user)) -> CarOutput:
    new_car = CarOutput(
        size=car.size,
        doors=car.doors,
        fuel=car.fuel,
        transmission=car.transmission,
        id=len(db) + 1,
    )
    db.append(new_car)
    save_db(db)
    return new_car


@app.delete("/api/cars/{id}", status_code=204)
def remove_car(id: int, user: str = Depends(get_current_user)) -> None:
    matches = [car for car in db if car.id == id]
    if matches:
        car = matches[0]
        db.remove(car)
        save_db(db)
    else:
        raise HTTPException(status_code=404, detail=f"No car with id={id}.")


@app.put("/api/cars/{id}")
def change_car(id: int, new_data: CarInput, user: str = Depends(get_current_user)) -> CarOutput:
    matches = [car for car in db if car.id == id]
    if matches:
        car = matches[0]
        car.fuel = new_data.fuel
        car.transmission = new_data.transmission
        car.size = new_data.size
        car.doors = new_data.doors
        save_db(db)
        return car
    else:
        raise HTTPException(status_code=404, detail=f"No car with id={id}.")


@app.post("/api/cars/{car_id}/trips")
def add_trip(car_id: int, trip: TripInput, user: str = Depends(get_current_user)) -> TripOutput:
    matches = [car for car in db if car.id == car_id]
    if matches:
        car = matches[0]
        new_trip = TripOutput(
            id=len(car.trips) + 1,
            start=trip.start,
            end=trip.end,
            description=trip.description,
        )
        car.trips.append(new_trip)
        save_db(db)
        return new_trip
    else:
        raise HTTPException(status_code=404, detail=f"No car with id={car_id}.")


if __name__ == "__main__":
    uvicorn.run("Carsharing:app", reload=True, host="0.0.0.0", port=8086)
