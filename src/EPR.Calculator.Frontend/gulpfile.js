const gulp = require('gulp');
const concat = require('gulp-concat');
const sass = require('gulp-sass')(require('sass'));
const path = require('path');

// Define paths to the SCSS files
const paths = {
    govuk: "node_modules/govuk-frontend/dist/govuk/",
    scss: [
        'assets/scss/all.scss'
    ],
    js: 'assets/js/',
    css: 'assets/css/',
    images: 'assets/images/',
    output: 'wwwroot/css'
};

const deprecationSuppressions = ["import", "mixed-decls", "global-builtin"];

let loadPaths = [
    path.join(__dirname, "node_modules"),
    path.join(__dirname, paths.govuk)
];

const sassOptions = {
    loadPaths: loadPaths,
    outputStyle: 'compressed',
    quietDeps: true,
    silenceDeprecations: deprecationSuppressions
};

// Task to copy scripts
gulp.task('copy-scripts', function () {
    gulp.src(path.join(paths.govuk, 'govuk-frontend.min.js'))
        .pipe(concat('govuk-frontend.min.js'))
        .pipe(gulp.dest('wwwroot/js', { overwrite: true }));

    return gulp.src(path.join(paths.js, '**/*'))
        .pipe(gulp.dest('wwwroot/js', { overwrite: true }));
});

// Task to copy images
gulp.task('copy-images', function () {
    gulp.src(path.join(paths.govuk, 'assets/images/*'))
        .pipe(gulp.dest('wwwroot/images', { overwrite: true }));

    return gulp.src(path.join(paths.images, '**/*'), {encoding:false})
        .pipe(gulp.dest('wwwroot/images', { overwrite: true }));
});

// Task to copy fonts
gulp.task('copy-fonts', function () {
    return gulp.src(path.join(paths.govuk, 'assets/fonts/*'))
        .pipe(gulp.dest('wwwroot/fonts', { overwrite: true }));
});

// Task to copy css
gulp.task('copy-css', function () {
    return gulp.src(path.join(paths.css, '**/*'))
        .pipe(gulp.dest('wwwroot/css', { overwrite: true }));
});

// Task to copy manifest
gulp.task('copy-govuk-manifest', function () {
    return gulp.src(path.join(paths.govuk, 'assets/manifest.json'))
        .pipe(gulp.dest('wwwroot/', { overwrite: true }));

});

// Task to compile and bundle SCSS into a single CSS file
gulp.task('compile-scss', function () {
    return gulp.src(paths.scss)
        .pipe(concat('application.css'))
        .pipe(sass(sassOptions).on('error', sass.logError))
        .pipe(gulp.dest(paths.output));
});

gulp.task('copy-govuk-rebrand', () => {
    return gulp.src(path.join(paths.govuk, '/assets/rebrand/**/*'), { base: path.join(paths.govuk, '/assets/rebrand') })
        .pipe(gulp.dest('wwwroot/rebrand'));
});

// Default task
gulp.task('build-frontend', gulp.series('copy-scripts', 'copy-images', 'copy-govuk-rebrand',
    'copy-fonts', 'copy-govuk-manifest', 'copy-css', 'compile-scss')); 