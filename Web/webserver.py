from flask import Flask
app = Flask(__name__)
app.config['DEBUG'] = True

@app.route('/')
def hello_world():
    return 'Hello World!'

@app.route('/patients/<int:patientid>/biodata/', methods=['GET', 'POST'])
def biodata_handler(patientid):
	print patientid
	return 'value page'

if __name__ == '__main__':
    app.run(host='192.168.0.105', port=5050)