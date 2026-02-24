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


@app.timer_trigger(schedule="0 0 * * * *", arg_name="my_timer", run_on_startup=True, use_monitor=True)
def timer_trigger_hourly(my_timer: func.TimerRequest) -> None:
    if my_timer.past_due:
        logging.warning("The timer is past due.")

    message = "Hello,  This Timmer triggered function executed successfully."
    logging.info(message)
