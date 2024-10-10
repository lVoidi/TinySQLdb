from faker import Faker

fake = Faker()
insert_statements = []

for i in range(1, 10001):
    name = fake.name()
    email = fake.email()
    insert_statements.append(f"INSERT INTO users VALUES ({i}, '{name}', '{email}');")

with open("inserts.txt", "w") as file:
    for statement in insert_statements:
        file.write(statement + "\n")

print("Sentencias INSERT generadas y guardadas en inserts.txt")
