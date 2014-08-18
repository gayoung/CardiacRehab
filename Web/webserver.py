from flask import Flask, request
from flask.ext.restful import Resource, Api
import json

app = Flask(__name__)
api = Api(app)

patient_contacts = {}
doc_contacts = {}

def flask_post_json():
    if (request.json != None):
        return request.json
    elif (request.data != None and request.data != ''):
        return json.loads(request.data)
    else:
        return json.loads(request.form.keys()[0])

class doctorContacts(Resource):
	def get(self, clinician_id):
		if(len(doc_contacts) != 0):
			return {"address": doc_contacts["address"], "name": doc_contacts["name"], 
			"id": doc_contacts["id"], "session": doc_contacts["session"], "assigned": doc_contacts["assigned_index"]}
		else:
			return "no data"
	
	def post(self, clinician_id):
		data = flask_post_json()
		doc_contacts["address"] = data['address']
		doc_contacts["name"] = data["name"]
		doc_contacts["id"] = data["id"]
		doc_contacts["session"] = data["session"]
		doc_contacts["assigned"] = data["assigned_index"]
		return {"address": data['address'], "name": data["name"], "id": data["id"],
		"session": data["session"], "assigned": data["assigned_index"]}
		
	def delete(self, clinician_id, patient_id):
		doc_contacts = {}
		return "Deleted"
		
class patientContacts(Resource):
	def get(self, clinician_id, patient_id):
		if(len(patient_contacts) != 0):
			return {"address": patient_contacts["address"], "name": patient_contacts["name"],
			"id": patient_contacts["id"], "session": patient_contacts["session"], "assigned": patient_contacts["assigned_index"]}
		else:
			return "no data"
	
	def post(self, clinician_id, patient_id):
		data = flask_post_json()
		patient_contacts["address"] = data["address"]
		patient_contacts["name"] = data["name"]
		patient_contacts["id"] = data["id"]
		patient_contacts["session"] = data["session"]
		patient_contacts["assigned"] = data["assigned_index"]
		return {"address": data['address'], "name": data["name"], "id": data["id"],
		"session": data["session"], "assigned": data["assigned_index"]}
		
	def delete(self, clinician_id, patient_id):
		patient_contacts = {}
		return "Deleted"

api.add_resource(doctorContacts, '/doctors/<int:clinician_id>/')
api.add_resource(patientContacts, '/doctors/<int:clinician_id>/patients/<int:patient_id>/')

if __name__ == '__main__':
    app.run(host='192.168.0.102', port=5050)