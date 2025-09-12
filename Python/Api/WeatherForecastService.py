from fastapi import FastAPI
from Entities.WeatherForecast import WeatherForecast
from datetime import timedelta, datetime
import random
import uvicorn


app = FastAPI()

summaries = ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"]

weatherforecasts = []

def generate_random_weatherforecast():
    if weatherforecasts:
        weatherforecasts.clear()
    # Weatherforecase for the next 7days 
    for day in range(7):
        weatherforecast = WeatherForecast(
            date = (datetime.now() + timedelta(days=day)).strftime("%Y-%m-%d"),
            summary = f"Today's weather is {summaries[random.randint(0,len(summaries))-1]}",
            tempC =  random.randint(29,35)
        )
        weatherforecasts.append(weatherforecast)

@app.get("/")
# Request Parameters
async def Welcome(name):
    """Return a friendly welcome message."""
    return {'message': f"Welcome {name} to Weather Forecast Service!"}

@app.get("/api/weatherforecast", response_model=list[WeatherForecast])
async def GetWeatherForecast():
    generate_random_weatherforecast()
    return weatherforecasts

if __name__ == "__main__":
    uvicorn.run("WeatherForecastService:app", port=8086, reload=True)