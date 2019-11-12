const fs = require('fs')

const data = {}
const dirs = fs.readdirSync('.').filter(e => e.startsWith('Tile'))
for (let dir of dirs) {
    console.log(dir)
    data[dir.replace('Tile1M_', '')] = JSON.parse(fs.readFileSync(dir + '/contents.json').toString())
}

fs.writeFileSync('./contents.json', JSON.stringify(data))
