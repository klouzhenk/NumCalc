export default {
    plugins: {
        'postcss-import': {},
        'postcss-nested': {},
        'postcss-custom-media': {},
        'postcss-preset-env': {
            stage: 1,
            features: {
                'custom-properties': false
            }
        },
    }
}