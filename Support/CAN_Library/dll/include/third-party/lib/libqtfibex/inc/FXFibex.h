#pragma once

#include "platform.h"
#include "asam_fibex_export.h"

#include "FXElements.h"

#if 0
#include "ProcessingInformation.h"
#include "ProjectType.h"
#include "Protocols.h"
#include "Requirements.h"
#include "VersionType.h"
#endif

#include "FXProcessingInformation.h"
#include "FXRequirements.h"

namespace ASAM {
namespace FIBEX {

/**
 * @brief element FIBEX
 *
 * Root element
 */
class ASAM_FIBEX_EXPORT FXFibex
{
public:
    FXFibex();
    ~FXFibex();

    /** load from XML DOM element */
    bool load(const QDomElement &element);

    /** element ELEMENTS */
    FXElements *m_elements = nullptr;

    /** element PROCESSING-INFORMATION */
    FXProcessingInformation *m_processingInformation = nullptr;

    /** element REQUIREMENTS */
    FXRequirements *m_requirements = nullptr;

#if 0
    /**
     * element PROJECT
     *
     * The project/vehicle described in the referencing file
     */
    ProjectType project;

    /** element PROTOCOLS */
    std::list<Protocols> protocols;

    /**
     * attribute VERSION
     *
     * To identify the schemas version that should be applied on the referencing file.
     * Note that this value is fixed on the current schema version to avoid confusion checking old FIBEX instances against a new FIBEX schema or oposite.
     */
    VersionType version;
#endif

};

}
}
