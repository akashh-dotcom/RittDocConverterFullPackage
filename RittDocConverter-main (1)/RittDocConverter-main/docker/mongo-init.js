// MongoDB Initialization Script
// ==============================
// This script runs when MongoDB container starts for the first time
// It creates necessary collections and indexes for the RittDocConverter

print('Initializing RittDocConverter database...');

// Switch to the application database
db = db.getSiblingDB('rittdoc_converter');

// =============================================================================
// Conversions Collection
// =============================================================================
print('Creating conversions collection...');

db.createCollection('conversions');

// Create indexes for efficient querying
db.conversions.createIndex(
    { "filename": 1, "start_time": 1 },
    { unique: true, name: "filename_start_time_idx" }
);

db.conversions.createIndex(
    { "status": 1 },
    { name: "status_idx" }
);

db.conversions.createIndex(
    { "start_time": -1 },
    { name: "start_time_desc_idx" }
);

db.conversions.createIndex(
    { "conversion_type": 1 },
    { name: "type_idx" }
);

db.conversions.createIndex(
    { "publisher": 1 },
    { name: "publisher_idx" }
);

print('Conversions collection ready with indexes.');

// =============================================================================
// Publishers Collection
// =============================================================================
print('Creating publishers collection...');

db.createCollection('publishers');

db.publishers.createIndex(
    { "name": 1 },
    { unique: true, name: "publisher_name_idx" }
);

// Insert default publishers
db.publishers.insertMany([
    {
        name: "O'Reilly Media",
        aliases: ["O'Reilly", "O'Reilly & Associates"],
        isbn_prefixes: ["978-1-491", "978-1-492", "978-0-596"],
        confidence_base: 95,
        known_issues: ["Code listings sometimes use images instead of <pre> tags"],
        notes: "Consistently high quality, excellent semantic structure",
        success_rate: 0.98,
        total_processed: 0,
        created_at: new Date().toISOString()
    },
    {
        name: "Wiley",
        aliases: ["John Wiley & Sons", "Wiley-Blackwell"],
        isbn_prefixes: ["978-1-118", "978-1-119", "978-0-470"],
        confidence_base: 90,
        known_issues: ["Complex tables occasionally need manual review"],
        notes: "High quality, good structure",
        success_rate: 0.94,
        total_processed: 0,
        created_at: new Date().toISOString()
    },
    {
        name: "Pearson",
        aliases: ["Pearson Education", "Addison-Wesley", "Prentice Hall"],
        isbn_prefixes: ["978-0-134", "978-0-321"],
        confidence_base: 88,
        known_issues: ["Academic texts often have complex equations"],
        notes: "Educational content, often includes pedagogical features",
        success_rate: 0.91,
        total_processed: 0,
        created_at: new Date().toISOString()
    },
    {
        name: "No Starch Press",
        aliases: ["No Starch"],
        isbn_prefixes: ["978-1-59327", "978-1-7185"],
        confidence_base: 92,
        known_issues: ["Heavy code listings (usually well-structured)"],
        notes: "Excellent technical publisher, programming focus",
        success_rate: 0.96,
        total_processed: 0,
        created_at: new Date().toISOString()
    },
    {
        name: "Unknown Publisher",
        aliases: [],
        isbn_prefixes: [],
        confidence_base: 50,
        known_issues: ["Unknown quality level", "Manual review recommended"],
        notes: "Default profile for unrecognized publishers",
        success_rate: 0.65,
        total_processed: 0,
        created_at: new Date().toISOString()
    }
]);

print('Publishers collection ready with default data.');

// =============================================================================
// Config Collection
// =============================================================================
print('Creating config collection...');

db.createCollection('config');

db.config.createIndex(
    { "_id": 1 },
    { name: "config_id_idx" }
);

// Insert default shared configuration
db.config.insertOne({
    _id: "shared_config",
    config: {
        storage: {
            type: "local",
            s3: {
                enabled: false,
                bucket_name: "",
                region: "us-east-1",
                input_prefix: "uploads"
            },
            local: {
                input_dir: "/app/uploads",
                output_dir: "/app/Output"
            }
        },
        database: {
            mongodb: {
                enabled: true,
                uri: "mongodb://mongodb:27017",
                database: "rittdoc_converter"
            }
        },
        processing: {
            mode: "automated",
            max_concurrent_jobs: 4,
            timeout_seconds: 3600,
            retry: {
                enabled: true,
                max_attempts: 3
            }
        },
        scheduler: {
            enabled: false,
            interval_minutes: 60,
            run_on_startup: false
        },
        quality: {
            auto_approve_threshold: 80,
            notify_threshold: 50,
            manual_review_threshold: 50
        },
        notifications: {
            enabled: false,
            channels: {
                email: { enabled: false },
                webhook: { enabled: false }
            }
        },
        logging: {
            level: "INFO",
            retention_days: 30
        },
        pipelines: {
            epub: {
                enabled: true,
                api_url: "http://epub-service:5001"
            }
        }
    },
    created_at: new Date().toISOString(),
    updated_at: new Date().toISOString()
});

print('Config collection ready with defaults.');

// =============================================================================
// Jobs Collection (for tracking async jobs)
// =============================================================================
print('Creating jobs collection...');

db.createCollection('jobs');

db.jobs.createIndex(
    { "job_id": 1 },
    { unique: true, name: "job_id_idx" }
);

db.jobs.createIndex(
    { "status": 1 },
    { name: "job_status_idx" }
);

db.jobs.createIndex(
    { "created_at": -1 },
    { name: "job_created_idx" }
);

// TTL index to auto-delete old completed jobs after 7 days
db.jobs.createIndex(
    { "completed_at": 1 },
    { expireAfterSeconds: 604800, name: "job_ttl_idx" }
);

print('Jobs collection ready.');

// =============================================================================
// Summary
// =============================================================================
print('');
print('===========================================');
print('  RittDocConverter Database Initialized!');
print('===========================================');
print('Collections created:');
print('  - conversions (with indexes)');
print('  - publishers (with default data)');
print('  - config (with default settings)');
print('  - jobs (with TTL index)');
print('');
