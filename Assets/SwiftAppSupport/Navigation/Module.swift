import Foundation

struct Module: Hashable, Identifiable {
    let id: String
    let eyebrow: String
    let title: String
    let unityCommand: String

    static let tableOfContents: [Module] = [
        Module(
            id: "cabinet_objects",
            eyebrow: "Objects",
            title: "Objects",
            unityCommand: "module:cabinet_objects"
        ),
        Module(
            id: "cabinet_help",
            eyebrow: "Help",
            title: "Help",
            unityCommand: "module:cabinet_help"
        )
    ]
}
