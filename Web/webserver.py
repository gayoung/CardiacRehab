from flask import Flask, request
from flask.ext.restful import Resource, Api
import json

app = Flask(__name__)
api = Api(app)

contacts = {}

def flask_post_json():
    if (request.json != None):
        return request.json
    elif (request.data != None and request.data != ''):
        return json.loads(request.data)
    else:
        return json.loads(request.form.keys()[0])

class HandleContacts(Resource):
    def get(self):
		if(len(contacts) != 0):
			return {"address": contacts["address"], "name": contacts["name"]}
		else:
			return "no data"

    def post(self):
		data = flask_post_json()
		contacts["address"] = data['ipAddress']
		contacts["name"] = data["name"]
		return {"ipaddress": data['ipAddress'], "name": data["name"]}

api.add_resource(HandleContacts, '/users/contacts/')

if __name__ == '__main__':
    app.run(host='192.168.0.105', port=5050)