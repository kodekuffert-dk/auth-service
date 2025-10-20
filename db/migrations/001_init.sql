
-- Enable the pgcrypto extension for UUID generation
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- Create the teams table
CREATE TABLE teams (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name TEXT NOT NULL UNIQUE,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Create the whitelist table
CREATE TABLE whitelist (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    email TEXT NOT NULL UNIQUE,
    status INTEGER NOT NULL DEFAULT 0,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    team_id UUID NOT NULL,
    CONSTRAINT fk_whitelist_team 
        FOREIGN KEY(team_id) 
        REFERENCES teams(id)
        ON DELETE CASCADE   
);

-- Create the users table
CREATE TABLE users (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    email TEXT NOT NULL UNIQUE,
    passwordhash TEXT NOT NULL,
    role TEXT NOT NULL,
    isemailconfirmed BOOLEAN NOT NULL DEFAULT FALSE,
    emailconfirmationtoken VARCHAR(255),
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    team_id UUID NULL,
    CONSTRAINT fk_user_team 
        FOREIGN KEY(team_id) 
        REFERENCES teams(id)
        ON DELETE CASCADE
);

-- Indexes for performance optimization
CREATE INDEX idx_whitelist_team_id ON whitelist(team_id);
CREATE INDEX idx_users_team_id ON users(team_id);
CREATE INDEX idx_users_email ON users(email);

-- Seed admin login into users table
INSERT INTO users (email, passwordhash, role, isemailconfirmed)
VALUES (
    'lany@ucn.dk',
    -- Construct the bcrypt hash using chr(36) to avoid $...$ tokens that DbUp treats as variables
    chr(36) || '2a' || chr(36) || '12' || chr(36) || 'Akf3n0f2.LtIeVxNYddL2OKC9SYESBI.0FsdccdhOELmuhgt5u/ti',
    'Administrator',
    TRUE
);