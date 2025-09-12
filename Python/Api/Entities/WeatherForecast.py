from pydantic import BaseModel

class WeatherForecast(BaseModel): 
        # using <type>|None = None declars the object property as nullable
        date: str|None = None
        tempC: int |None = None
        summary: str |None = None
    
