import bcrypt

# Brugeren indtaster en adgangskode
password = input("Indtast adgangskode: ").encode('utf-8')

# Generér en salt (cost = 12 er en god standard)
salt = bcrypt.gensalt(rounds=12)

# Hash adgangskoden
hashed = bcrypt.hashpw(password, salt)

# Print resultatet
print("Hashed password:")
print(hashed.decode('utf-8'))
