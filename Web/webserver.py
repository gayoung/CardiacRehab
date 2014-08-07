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

class doctorContacts(Resource):
    def get(self, clinician_id):
		if(len(contacts) != 0):
			return {clinician_id: {"address": contacts["address"], "name": contacts["name"]}}
		else:
			return "no data"

    def post(self, clinician_id):
		data = flask_post_json()
		contacts["address"] = data['address']
		contacts["name"] = data["name"]
		return {clinician_id : {"address": data['address'], "name": data["name"]}}
		
class patientContacts(Resource):
	def get(self, clinician_id, patient_id):
		return "get test"
	
	def post(self, clinician_id, patient_id):
		data = flask_post_json()
		print data["address"]
		print data["name"]
		return "post test"

api.add_resource(doctorContacts, '/doctors/<int:clinician_id>/')
api.add_resource(patientContacts, '/doctors/<int:clinician_id>/patients/<int:patient_id>/')

if __name__ == '__main__':
    app.run(host='192.168.0.105', port=5050)