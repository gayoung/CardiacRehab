/* 
This code is needed for the other computers to connect to this local db server. 
For now, this query needs to list all IP that will attempt to connect to this db server.
*/
GRANT ALL PRIVILEGES ON *.* TO 'root'@'192.168.0.101' IDENTIFIED BY '';
FLUSH PRIVILEGES;

DROP TABLE IF EXISTS hr_data;
DROP TABLE IF EXISTS ox_data;
DROP TABLE IF EXISTS ui_data;
DROP TABLE IF EXISTS bp_data;
DROP TABLE IF EXISTS note_data;
DROP TABLE IF EXISTS ecg_data;
DROP TABLE IF EXISTS power_data;
DROP TABLE IF EXISTS wheel_data;
DROP TABLE IF EXISTS crank_data;

DROP TABLE IF EXISTS patient_session;
DROP TABLE IF EXISTS clinical_staff;
DROP TABLE IF EXISTS patient;
DROP TABLE IF EXISTS authentication;

/* Role : patient/doctor/nurses...etc */
CREATE TABLE IF NOT EXISTS authentication(
id int(10) NOT NULL AUTO_INCREMENT,
username varchar(50) NOT NULL,
password varchar(50) NOT NULL,
role varchar(20) NOT NULL,
PRIMARY KEY(id)
);

/* Current database schema assumes that the patients only have one
doctor associated with them... possibly not a good assumption to make? */

CREATE TABLE IF NOT EXISTS patient(
patient_id int(10) NOT NULL,
fname varchar(50) NOT NULL,
lname varchar(50) NOT NULL,
date_joined datetime,
date_birth datetime,
email varchar(100) NOT NULL,
staff_id int(10) NOT NULL,
PRIMARY KEY(patient_id),
FOREIGN KEY(patient_id) REFERENCES authentication(id) ON DELETE CASCADE,
FOREIGN KEY(staff_id) REFERENCES authentication(id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS clinical_staff(
staff_id int(10) NOT NULL,
fname varchar(50) NOT NULL,
lname varchar(50) NOT NULL,
date_joined datetime,
email varchar(100) NOT NULL,
PRIMARY KEY(staff_id),
FOREIGN KEY(staff_id) REFERENCES authentication(id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS patient_session(
id int(10) NOT NULL AUTO_INCREMENT,
patient_id int(10),
staff_id int(10),
date_start datetime NOT NULL,
date_end datetime,
chosen_level int(10),
PRIMARY KEY(id),
FOREIGN KEY(patient_id) REFERENCES authentication(id) ON DELETE CASCADE,
FOREIGN KEY(staff_id) REFERENCES authentication(id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS hr_data(
id int(10) NOT NULL AUTO_INCREMENT,
heart_rate varchar(100) NOT NULL, /* varchar b/c it will be encrypted */
session_id int(10) NOT NULL,
PRIMARY KEY(id),
FOREIGN KEY(session_id) REFERENCES patient_session(id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS ox_data(
id int(10) NOT NULL AUTO_INCREMENT,
oxy_sat varchar(100) NOT NULL, /* varchar b/c it will be encrypted */
session_id int(10) NOT NULL,
PRIMARY KEY(id),
FOREIGN KEY(session_id) REFERENCES patient_session(id) ON DELETE CASCADE
);

/* user intensity data */
CREATE TABLE IF NOT EXISTS ui_data(
id int(10) NOT NULL AUTO_INCREMENT,
ui_value int(10) NOT NULL,
session_id int(10) NOT NULL,
PRIMARY KEY(id),
FOREIGN KEY(session_id) REFERENCES patient_session(id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS bp_data(
id int(10) NOT NULL AUTO_INCREMENT,
systolic varchar(100) NOT NULL, /* varchar b/c it will be encrypted */
diastolic varchar(100) NOT NULL, /* varchar b/c it will be encrypted */
session_id int(10) NOT NULL,
PRIMARY KEY(id),
FOREIGN KEY(session_id) REFERENCES patient_session(id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS note_data(
id int(10) NOT NULL AUTO_INCREMENT,
note varchar(1000) NOT NULL, /* varchar b/c it will be encrypted */
date_stamped datetime NOT NULL,
session_id int(10) NOT NULL,
PRIMARY KEY(id),
FOREIGN KEY(session_id) REFERENCES patient_session(id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS ecg_data(
id int(10) NOT NULL AUTO_INCREMENT,
ecg_data varchar(300) NOT NULL, /* varchar b/c it will be encrypted */
session_id int(10) NOT NULL,
PRIMARY KEY(id),
FOREIGN KEY(session_id) REFERENCES patient_session(id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS power_data(
id int(10) NOT NULL AUTO_INCREMENT,
power_value int(10) NOT NULL,
resistance_lv int(10) NOT NULL,
session_id int(10) NOT NULL,
PRIMARY KEY(id),
FOREIGN KEY(session_id) REFERENCES patient_session(id) ON DELETE CASCADE
);
CREATE TABLE IF NOT EXISTS wheel_data(
id int(10) NOT NULL AUTO_INCREMENT,
wheel_value int(10) NOT NULL,
session_id int(10) NOT NULL,
PRIMARY KEY(id),
FOREIGN KEY(session_id) REFERENCES patient_session(id) ON DELETE CASCADE
);
CREATE TABLE IF NOT EXISTS crank_data(
id int(10) NOT NULL AUTO_INCREMENT,
crank_value int(10) NOT NULL, 
session_id int(10) NOT NULL,
PRIMARY KEY(id),
FOREIGN KEY(session_id) REFERENCES patient_session(id) ON DELETE CASCADE
);

INSERT INTO authentication (username, password, role) VALUES('admin', 'admin', 'Admin');

INSERT INTO authentication (username, password, role) VALUES('***', '*****', 'iHealth');

INSERT INTO authentication (username, password, role) VALUES('patient1', 'test', 'Patient');
INSERT INTO authentication (username, password, role) VALUES('doctor0', 'test', 'Doctor-offline');
INSERT INTO authentication (username, password, role) VALUES('doctor1', 'test', 'Doctor');

INSERT INTO clinical_staff VALUES(
(SELECT id FROM authentication WHERE username = 'doctor1' AND password = 'test'),
'Matt', 'Smith', NOW(), 'doc@example.com'
);

INSERT INTO clinical_staff VALUES(
(SELECT id FROM authentication WHERE username = 'doctor0' AND password = 'test'),
'Offline', 'Offline', NOW(), 'offline@example.com'
);

INSERT INTO patient VALUES(
(SELECT id FROM authentication WHERE username = 'patient1' AND password = 'test'),
'Amelia', 'Pond', NOW(), '1988-01-01', 'test@example.com',
(SELECT id FROM authentication WHERE username = 'doctor1' AND password = 'test')
);