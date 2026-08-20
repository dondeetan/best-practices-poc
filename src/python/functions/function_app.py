import azure.functions as func
import logging

app = func.FunctionApp()

@app.route(route="httptriggerwelcomeanonymous", auth_level=func.AuthLevel.ANONYMOUS)
def http_trigger_welcome(req: func.HttpRequest) -> func.HttpResponse:
    logging.info('Python HTTP trigger function processed a request.')

    name = req.params.get('name')
    if not name:
        try:
            req_body = req.get_json()
        except ValueError:
            pass
        else:
            name = req_body.get('name')

    if name:
        return func.HttpResponse(f"Hello, {name}. This HTTP triggered function executed successfully.")
    else:
        return func.HttpResponse(
             "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response.",
             status_code=200
        )

# NCRONTAB: second minute hour day month day-of-week
# "0 0 */12 * * *" runs at 00:00 and 12:00 every day (every 12 hours).
@app.timer_trigger(schedule="0 0 */12 * * *", arg_name="my_timer", run_on_startup=False, use_monitor=True)
def timer_trigger_hourly(my_timer: func.TimerRequest) -> None:
    if my_timer.past_due:
        logging.warning("The timer is past due.")

    message = "Hello,  This Timmer triggered function executed successfully."
    logging.info(message)
