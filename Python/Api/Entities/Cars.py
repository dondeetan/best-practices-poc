import json
import os
from pydantic import BaseModel

class TripInput(BaseModel):
    start: int
    end: int
    description: str
    employeeid: int | None = 1

class TripOutput(TripInput):
    id: int
    

class CarInput(BaseModel):
    size: str
    fuel: str | None = "electric"
    doors: int
    transmission: str | None = "auto"
    employeeid: int | None = 1

    model_config = {
        "json_schema_extra": {
            "examples": [
                {
                "size": "m",
                "doors": 5,
                "transmission": "manual",
                "fuel": "hybrid"
                }
            ]
        }
    }

class CarOutput(CarInput):
    id: int
    trips: list[TripOutput] = [] # noqa (turns off an incorrect pycharm warning)

base_dir = os.path.dirname(os.path.abspath(__file__))
file_path = os.path.join(base_dir, "../Sources", "cars.json")

def load_db() -> list[CarOutput]:
    """Load a list of Car objects from a JSON file"""
    with open(file_path) as f:
        return [CarOutput.model_validate(obj) for obj in json.load(f)]

def save_db(cars: list[CarOutput]):
    with open(file_path, 'w') as f:
        json.dump([car.model_dump() for car in cars], f, indent=4)